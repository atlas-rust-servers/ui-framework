using Network;
using Oxide.Ext.UiFramework.Exceptions;
using Oxide.Ext.UiFramework.Json;
using Oxide.Ext.UiFramework.Libraries;

namespace Oxide.Ext.UiFramework.Animation;

public abstract class SendableAnimation : BaseAnimation, ISendableAnimation
{
    public bool IsSending { get; private set; }

    private SendInfo _send;
    public SendInfo Send
    {
        get => _send;
        set
        {
            if (!IsSending)
            {
                _send = value;
                IsSending = true;
                return;
            }

            throw new AnimationException($"{nameof(ISendableAnimation)}.{nameof(Send)}.set cannot be called after it has been set.");
        }
    }

    public virtual void Serialize(JsonFrameworkWriter writer)
    {
        for (int index = 0; index < Children.Count; index++)
        {
            if (Children[index] is ISendableAnimation animation)
            {
                animation.Serialize(writer);
            }
        }
    }

    public void AddPlayer(Connection connection)
    {
        if (connection is null)
        {
            return;
        }
        
        if (this.IsSinglePlayer())
        {
            throw new AnimationException($"{nameof(ISendableAnimation)}.{nameof(AddPlayer)} cannot be called on a single player animation.");
        }

        if (!Send.connections.Contains(connection))
        {
            Send.connections.Add(connection);
        }
    }

    public void RemovePlayer(ulong playerId)
    {
        //RemovePlayer can be called from the AnimationTrackerChannel thread while the animation
        //is concurrently being pooled on the animation thread (EnterPool resets _send to default
        //and clears IsSending). Bail out if we're no longer sending and snapshot the struct once
        //so a concurrent reset can't null out connection/connections mid-method.
        if (!IsSending)
        {
            return;
        }

        SendInfo send = Send;
        if (send.connections != null)
        {
            for (int index = send.connections.Count - 1; index >= 0; index--)
            {
                Connection connection = send.connections[index];
                if (connection.userid == playerId)
                {
                    send.connections.RemoveAt(index);
                    break;
                }
            }

            if (send.connections.Count == 0)
            {
                CancelAnimation();
            }

            return;
        }

        if (send.connection != null && send.connection.userid == playerId)
        {
            CancelAnimation();
        }
    }
    
    public override ISendableAnimation GetSendable() => IsSending ? this : base.GetSendable();

    protected override void EnterPool()
    {
        base.EnterPool();
        if (Send.connections != null)
        {
            // The connections list is rented from the dedicated UiPool.Connections pool in
            // SendInfoBuilder, so it must be returned there. Freeing it to PluginPool (or any
            // other pool) corrupts pool accounting and can lead to the same list instance being
            // handed out twice (aliasing) -> "Collection was modified" during the next send copy.
            UiPool.Connections.FreeList(Send.connections);
        }
        _send = default;
        IsSending = false;
    }
}