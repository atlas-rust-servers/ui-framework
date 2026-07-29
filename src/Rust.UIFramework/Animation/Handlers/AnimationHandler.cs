using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Network;
using Oxide.Ext.UiFramework.Config;
using Oxide.Ext.UiFramework.Exceptions;
using Oxide.Ext.UiFramework.Extensions;
using Oxide.Ext.UiFramework.Helpers;
using Oxide.Ext.UiFramework.Json;
using Oxide.Ext.UiFramework.Logging;
using Oxide.Ext.UiFramework.Plugins;
using Oxide.Ext.UiFramework.Pooling;
using Oxide.Ext.UiFramework.Threading;
using Oxide.Ext.UiFramework.Types;
using UnityEngine;

namespace Oxide.Ext.UiFramework.Animation;

internal class AnimationHandler : ISingleton
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly AutoResetEvent _reset = new(false);
    private int _sendCount;
    private readonly IUiLogger _logger = Singleton<UiLoggerFactory>.Instance.CreateExtensionLogger<AnimationHandler>();
    private bool _isPaused = true;

    private AnimationHandler()
    {
        Thread thread = new(AnimationLoop)
        {
            IsBackground = true
        };
        thread.Start();
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
        _reset.Set();
        _logger.Debug("Adding animation {0}", animation.Id);
    }
    
    private void AnimationLoop()
    {
        CancellationToken token = _cancellationTokenSource.Token;
        
        while (!token.IsCancellationRequested)
        {
            try
            {
                float timeTaken = TickAnimation();
                DelayTillNextAnimationFrame(timeTaken);
            }
            catch (Exception ex)
            {
                Thread.Sleep(UiFrameworkConfig.Instance.Animations.UpdateRate);
                _logger.Exception("An error occurred while processing animations", ex);
            }
        }
    }
    
    internal float TickAnimation()
    {
        float startTime = Time.realtimeSinceStartup;
        Singleton<AnimationTime>.Instance.UpdateTime(startTime, _isPaused);
        _logger.Debug("Processing {0} animations. Delta: {1:0.0000} seconds", Singleton<AnimationData>.Instance.Count, Singleton<AnimationTime>.Instance.DeltaTime);
        ProcessAnimations();
        _logger.Debug("Processed animations. {0} remaining", Singleton<AnimationData>.Instance.Count);
        float endTime = Time.realtimeSinceStartup;
        return endTime - startTime;
    }

    internal void DelayTillNextAnimationFrame(float timeTaken)
    {
        if (Singleton<AnimationData>.Instance.Count == 0)
        {
            _sendCount++;
            int timeout = GetSleepDuration();
            if (timeout == -1)
            {
                _isPaused = true;
            }
            _reset.WaitOne(timeout);
        }
        else
        {
            _sendCount = 0;
            _isPaused = false;
            int processDuration = Mathf.RoundToInt(timeTaken * 1000);
            int sleepDuration = Mathf.Max(UiFrameworkConfig.Instance.Animations.UpdateRate - processDuration, 1);
            Thread.Sleep(sleepDuration);
        }
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
                HandleSendAnimation(writer, playerAnimations.Send);
            else
                writer.Dispose();
        }
        
        foreach (ISendableAnimation animation in Singleton<AnimationData>.Instance.GroupAnimations.GetEnumeratorPooled(UiFrameworkPlugin.Instance).Values)
        {
            JsonFrameworkWriter writer = Create();
            if (ProcessAnimation(animation, writer))
                HandleSendAnimation(writer, animation.Send);
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
    
    private static void HandleSendAnimation(JsonFrameworkWriter writer, SendInfo send)
    {
        if (ThreadingHelper.AnimationsMultiThreaded)
        {
            SendAnimations(writer, send);
        }
        else
        {
            SendAnimationMainThread(writer, send).Forget();
        }
    }

    private static async UniTaskVoid SendAnimationMainThread(JsonFrameworkWriter writer, SendInfo send)
    {
        try
        {
            await UniTask.SwitchToMainThread();
            SendAnimations(writer, send);
        }
        finally
        {
            await UniTask.SwitchToThreadPool();
            writer.TryDispose();
        }
    }
    
    private static void SendAnimations(JsonFrameworkWriter writer, SendInfo send)
    {
        writer.WriteEndArray();
        RpcFunctions.SendAddUi(send, writer);
        writer.Dispose();
    }
    
    private int GetSleepDuration()
    {
        if (_sendCount > 4)
        {
            return -1;
        }
        
        return 25 + (1 << _sendCount);
    }

    public void OnPlayerDisconnected(ulong playerId) => Singleton<AnimationData>.Instance.OnPlayerDisconnected(playerId);
    internal void OnPluginUnloaded(IUiFrameworkPlugin plugin) => Singleton<AnimationData>.Instance.OnPluginUnloaded(plugin);
    internal void OnServerShutdown()
    {
        _cancellationTokenSource.Cancel();
    }
}