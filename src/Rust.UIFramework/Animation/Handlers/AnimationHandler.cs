using System;
using Network;
using Oxide.Ext.UiFramework.Config;
using Oxide.Ext.UiFramework.Exceptions;
using Oxide.Ext.UiFramework.Extensions;
using Oxide.Ext.UiFramework.Helpers;
using Oxide.Ext.UiFramework.Json;
using Oxide.Ext.UiFramework.Logging;
using Oxide.Ext.UiFramework.Plugins;
using Oxide.Ext.UiFramework.Pooling;
using Oxide.Ext.UiFramework.Types;
using UnityEngine;

namespace Oxide.Ext.UiFramework.Animation;

internal class AnimationHandler : ISingleton
{
    private readonly IAnimationHandler _handler;
    private readonly IUiLogger _logger = Singleton<UiLoggerFactory>.Instance.CreateExtensionLogger<AnimationHandler>();

    private AnimationHandler()
    {
        if (UiFrameworkConfig.Instance.Threading.EnableAnimationThread)
        {
            _handler = new ThreadedAnimationHandler();
        }
        else
        {
            _handler = SingletonBehavior<BehaviorAnimationHandler>.Instance;
        }
        
        _handler.OnInit(this);
    }
    
    public void EnqueueAnimation(ISendableAnimation animation, SendInfo send)
    {
        if (animation == null) throw new ArgumentNullException(nameof(animation));

        //A valid enqueue always transitions Init -> Queued, so the animation must be in the Init
        //state here. Any other state means it has already been processed and we must not enqueue
        //it again. This covers two cases:
        // 1. Terminal states (Cancelled / Completed / Timeout / Pooled): the animation may have
        //    been cancelled / completed / disposed before reaching the SendHandler thread (e.g.
        //    ImageDownloadAnimationHandler.CancelPreviousUpdates cancels a prior image animation
        //    when a new one is queued for the same player+panel). It was never visible to the
        //    client, so there is nothing to send.
        // 2. Already-sending states (Queued / Delayed / Running): the same animation instance was
        //    enqueued more than once (e.g. a builder containing animations sent via multiple
        //    AddUi calls, the same animation added twice, or shared across combined sub-builders).
        //    Re-enqueuing would set Send a second time while IsSending is true and throw.
        //In both cases skip instead of throwing so one animation can't crash the send channel.
        if (animation.State != AnimationState.Init)
        {
            _logger.Debug("Skipping EnqueueAnimation for ID: {0} Plugin: {1} - not in the {2} state (current state: {3})", animation.Id, animation.Plugin, AnimationState.Init, animation.State);
            return;
        }

        animation.Send = send;
        animation.ChangeState(AnimationState.Queued);
        AnimationException.ThrowIfMissingSend(animation);
        Singleton<AnimationData>.Instance.OnAnimationQueued(animation);
        _handler.OnAnimationQueued();
        _logger.Debug("Adding animation {0}", animation.Id);
    }
    
    internal float TickAnimation(bool wasPaused)
    {
        float startTime = Time.realtimeSinceStartup;
        Singleton<AnimationTime>.Instance.UpdateTime(startTime, wasPaused);
        _logger.Debug("Processing {0} animations. Delta: {1:0.0000} seconds", Singleton<AnimationData>.Instance.Count, Singleton<AnimationTime>.Instance.DeltaTime);
        ProcessAnimations();
        _logger.Debug("Processed animations. {0} remaining", Singleton<AnimationData>.Instance.Count);
        float endTime = Time.realtimeSinceStartup;
        return endTime - startTime;
    }
    
    private void ProcessAnimations()
    {
        foreach (PlayerAnimationData playerAnimations in Singleton<AnimationData>.Instance.PlayerAnimations.GetEnumeratorPooled(UiFrameworkPlugin.Instance).Values)
        {
            JsonFrameworkWriter writer = Create();
            bool hasAnimations = false;
            foreach (ISendableAnimation animation in playerAnimations.Animations.GetEnumeratorPooled(UiFrameworkPlugin.Instance).Values)
            {
                if (ProcessAnimation(animation, writer))
                    hasAnimations = true;
            }

            if (hasAnimations)
                SendAnimations(writer, playerAnimations.Send);
            else
                writer.Dispose();
        }
        
        foreach (ISendableAnimation animation in Singleton<AnimationData>.Instance.GroupAnimations.GetEnumeratorPooled(UiFrameworkPlugin.Instance).Values)
        {
            JsonFrameworkWriter writer = Create();
            if (ProcessAnimation(animation, writer))
                SendAnimations(writer, animation.Send);
            else
                writer.Dispose();
        }

        Singleton<AnimationData>.Instance.CleanupCompletedAnimations();
    }

    private bool ProcessAnimation(ISendableAnimation animation, JsonFrameworkWriter writer)
    {
        _logger.Debug("Processing Animation {0}", animation.Id);

        try
        {
            if (animation.Parent is null)
            {
                if (animation.State == AnimationState.Pooled)
                {
                    return false;
                }
                
                if (animation.State == AnimationState.Queued)
                {
                    animation.OnStarted();
                }
                
                animation.OnTick();
                UiFrameworkExtension.GlobalLogger.Debug($"{nameof(AnimationHandler)}.{nameof(ProcessAnimation)} ID: {{0}} HasChanged: {{1}}", animation.Id, animation.HasChanged);
                if (animation.HasChanged && animation.State != AnimationState.Cancelled)
                {
                    animation.Serialize(writer);
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            if (animation is not IPoolable { IsPooled: true })
            {
                _logger.Exception("An error occured processing animation ID: {0} Plugin: {1}. Cancelling Animation.", animation.Id, animation.Plugin, ex);
                animation.CancelAnimation();
            }
            else
            {
                _logger.Exception("An error occured processing animation. Animation is DISPOSED.", ex);
            }
        }

        return false;
    }

    private static JsonFrameworkWriter Create()
    {
        JsonFrameworkWriter writer = JsonFrameworkWriter.Create(UiFrameworkPlugin.Instance);
        writer.WriteStartArray();
        return writer;
    }

    private static void SendAnimations(JsonFrameworkWriter writer, SendInfo send)
    {
        writer.WriteEndArray();
        RpcFunctions.SendAddUi(send, writer);
        writer.Dispose();
    }

    public void OnPlayerDisconnected(ulong playerId) => Singleton<AnimationData>.Instance.OnPlayerDisconnected(playerId);
    internal void OnPluginUnloaded(IUiFrameworkPlugin plugin) => Singleton<AnimationData>.Instance.OnPluginUnloaded(plugin);
    internal void OnServerShutdown() => _handler.OnServerShutdown();
}