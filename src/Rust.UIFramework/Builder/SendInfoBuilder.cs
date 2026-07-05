using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Network;
using Oxide.Ext.UiFramework.Animation;
using Oxide.Ext.UiFramework.Libraries;
using Oxide.Ext.UiFramework.Types;

namespace Oxide.Ext.UiFramework.Builder;

internal static class SendInfoBuilder
{
    private const sbyte UiChannel = 3;
    private const sbyte AnimationsChannel = 4;
    private const sbyte PreCache = 5;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static SendInfo Get(BasePlayer player)
    {
        if (!player) throw new ArgumentNullException(nameof(player));
        return Get(player.Connection);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static SendInfo GetForPrecache(BasePlayer player)
    {
        if (!player) throw new ArgumentNullException(nameof(player));
        return Get(player.Connection, PreCache);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static SendInfo Get(Connection connection) => Get(connection, UiChannel);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static SendInfo Get(Connection connection, sbyte channel)
    {
        if (connection == null) throw new ArgumentNullException(nameof(connection));
        return new SendInfo(connection)
        {
            channel = channel
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static SendInfo Get(IEnumerable<Connection> connections)
    {
        if (connections == null) throw new ArgumentNullException(nameof(connections));
        List<Connection> pooledConnection = RentDistinct(connections);
        CopyConnected(connections, pooledConnection);

        return new SendInfo(pooledConnection)
        {
            channel = UiChannel
        };
    }

    internal static SendInfo GetForAnimations(SendInfo info)
    {
        sbyte channel = Singleton<AnimationTime>.Instance.AnimationsEnabled ? AnimationsChannel : UiChannel;
        return GetForChannel(info, channel);
    }

    internal static SendInfo GetForUi(SendInfo info)
    {
        return GetForChannel(info, UiChannel);
    }

    private static SendInfo GetForChannel(SendInfo info, sbyte channel)
    {
        if (info.connection != null)
        {
            return new SendInfo(info.connection)
            {
                channel = channel
            };
        }

        List<Connection> connections = RentDistinct(info.connections);
        CopyConnected(info.connections, connections);

        return new SendInfo(connections)
        {
            channel = channel
        };
    }

    // Rents a destination list from the dedicated, framework-only Connections pool. Because that
    // pool is never exposed to plugins, the list can't already be owned by external code that is
    // passing it in as the source. As defense in depth we still ensure the rented list is a
    // different instance than the source; copying source -> destination into the same list would
    // otherwise throw "Collection was modified" (deterministically, single-threaded). If that
    // ever happens we leave the aliased list alone (the caller owns it) and rent another.
    private static List<Connection> RentDistinct(IEnumerable<Connection> source)
    {
        List<Connection> list = UiPool.Connections.GetList<Connection>();
        if (ReferenceEquals(list, source))
        {
            list = UiPool.Connections.GetList<Connection>();
        }

        return list;
    }

    // Copies only connected connections into the destination list. `destination` is guaranteed by
    // RentDistinct to be a different instance than `source`, so adding here never mutates what we
    // iterate. The source may still be mutated concurrently by another thread (e.g. the animation
    // tracker thread calling RemovePlayer/AddPlayer on a shared connections list), so we:
    //   1. Prefer index iteration for IList/IReadOnlyList sources, avoiding List<T>.Enumerator's
    //      version check that throws "Collection was modified".
    //   2. Snapshot the count once and bound-check each access so a concurrent removal can't run
    //      us past the end, and wrap everything so any remaining race degrades to "send to who we
    //      already gathered" instead of throwing up through the caller's update loop.
    // A torn read is harmless here: every candidate is re-checked for `connected`.
    private static void CopyConnected(IEnumerable<Connection> source, List<Connection> destination)
    {
        try
        {
            if (source is IReadOnlyList<Connection> list)
            {
                for (int i = 0, count = list.Count; i < count && i < list.Count; i++)
                {
                    Connection connection = list[i];
                    if (connection is { connected: true })
                    {
                        destination.Add(connection);
                    }
                }

                return;
            }

            foreach (Connection connection in source)
            {
                if (connection is { connected: true })
                {
                    destination.Add(connection);
                }
            }
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentOutOfRangeException)
        {
            // Source was mutated mid-copy by another thread; keep what we already gathered.
        }
    }
}