using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Oxide.Ext.UiFramework.Callbacks;

internal static class AsyncExecutioner
{
    private static Task _executioner;
    private static readonly ConcurrentQueue<BaseAsyncCallback> CallbacksQueue = new();

    public static void Schedule(BaseAsyncCallback callback)
    {
        CallbacksQueue.Enqueue(callback);

        if (_executioner == null || _executioner.IsCompleted)
            _executioner = Task.Run(ExecuteQueue);
    }

    private static void ExecuteQueue()
    {
        while (CallbacksQueue.TryDequeue(out BaseAsyncCallback callback))
            callback.CallbackInternal();
    }
}