using Oxide.Ext.UiFramework.Libraries;

namespace Rust.UiFramework.UnitTests.Libraries.Channels;

public class UiChannelTests
{
    [Fact]
    public void ConcurrentEnqueue_CompletesEveryRequestWithinWorkerLimit()
    {
        const int REQUEST_COUNT = 200;
        const int WORKER_LIMIT = 3;
        using CountdownEvent completed = new(REQUEST_COUNT);
        UiChannel<ChannelRequest> channel = new(new UiChannelOptions(true, WORKER_LIMIT));
        int active = 0;
        int maximumActive = 0;
        int processed = 0;

        try
        {
            Parallel.For(0, REQUEST_COUNT, Enqueue);

            Assert.True(completed.Wait(TimeSpan.FromSeconds(5)));
            Assert.Equal(REQUEST_COUNT, processed);
            Assert.InRange(maximumActive, 1, WORKER_LIMIT);
        }
        finally
        {
            channel.Stop();
        }

        void Enqueue(int index)
        {
            channel.Enqueue(new ChannelRequest(Process, Complete));
        }

        void Process()
        {
            int current = Interlocked.Increment(ref active);
            int previous;
            do
            {
                previous = Volatile.Read(ref maximumActive);
                if (previous >= current)
                    break;
            }
            while (Interlocked.CompareExchange(ref maximumActive, current, previous) != previous);

            Thread.Yield();
            Interlocked.Increment(ref processed);
            Interlocked.Decrement(ref active);
        }

        void Complete()
        {
            completed.Signal();
        }
    }

    [Fact]
    public async Task Stop_DoesNotRestartWorkersWithQueuedRequests()
    {
        using ManualResetEventSlim started = new();
        using ManualResetEventSlim release = new();
        UiChannel<ChannelRequest> channel = new(new UiChannelOptions(false, 1));
        int completed = 0;
        int pendingProcessed = 0;
        Task producer = Task.Run(EnqueueFirst);

        try
        {
            Assert.True(started.Wait(TimeSpan.FromSeconds(5)));
            channel.Enqueue(new ChannelRequest(ProcessPending, Complete));
            channel.Stop();
        }
        finally
        {
            channel.Stop();
            release.Set();
            await producer.WaitAsync(TimeSpan.FromSeconds(5));
        }

        Assert.Equal(1, completed);
        Assert.Equal(0, pendingProcessed);

        void EnqueueFirst()
        {
            channel.Enqueue(new ChannelRequest(ProcessFirst, Complete));
        }

        void ProcessFirst()
        {
            started.Set();
            if (!release.Wait(TimeSpan.FromSeconds(5)))
                throw new TimeoutException("The running channel request was not released.");
        }

        void ProcessPending()
        {
            Interlocked.Increment(ref pendingProcessed);
        }

        void Complete()
        {
            Interlocked.Increment(ref completed);
        }
    }

    private sealed class ChannelRequest : IUiChannelObject
    {
        private readonly Action _process;
        private readonly Action _complete;

        public ChannelRequest(Action process, Action complete)
        {
            _process = process;
            _complete = complete;
        }

        public ProcessResult Process()
        {
            _process();
            return ProcessResult.Success;
        }

        public void OnCompleted()
        {
            _complete();
        }
    }
}
