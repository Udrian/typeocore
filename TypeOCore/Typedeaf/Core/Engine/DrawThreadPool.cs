using System.Collections.Concurrent;
using TypeOEngine.Typedeaf.Core.Engine.Graphics;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine
    {
        internal sealed class DrawThreadPool : IDisposable
        {
            private readonly object _sync = new();
            private readonly Worker[] _workers;
            private bool _disposed;

            public DrawThreadPool(string name, int threadCount)
            {
                if (threadCount < 1)
                    throw new ArgumentOutOfRangeException(nameof(threadCount));

                _workers = Enumerable.Range(0, threadCount)
                    .Select(index => new Worker($"{name} Draw {index + 1}"))
                    .ToArray();

                try
                {
                    foreach (var worker in _workers)
                        worker.Thread.Start();
                }
                catch
                {
                    foreach (var worker in _workers)
                        worker.Queue.CompleteAdding();
                    foreach (var worker in _workers)
                    {
                        if (worker.Thread.IsAlive)
                            worker.Thread.Join();
                        worker.Queue.Dispose();
                    }
                    throw;
                }
            }

            public void Run(int itemCount, Action<int> action)
            {
                if (itemCount == 0)
                    return;
                if (action == null)
                    throw new ArgumentNullException(nameof(action));

                if (CanvasDrawCommandQueue.IsWorkerThread)
                {
                    for (var index = 0; index < itemCount; index++)
                        action(index);
                    return;
                }

                lock (_sync)
                {
                    ObjectDisposedException.ThrowIf(_disposed, this);
                    var workerCount = Math.Min(itemCount, _workers.Length);
                    var batch = new DrawBatch(itemCount, workerCount, action);
                    for (var i = 0; i < workerCount; i++)
                        _workers[i].Queue.Add(batch);

                    batch.Wait();
                }
            }

            public void Dispose()
            {
                lock (_sync)
                {
                    if (_disposed)
                        return;

                    _disposed = true;
                    foreach (var worker in _workers)
                        worker.Queue.CompleteAdding();
                    foreach (var worker in _workers)
                    {
                        worker.Thread.Join();
                        worker.Queue.Dispose();
                    }
                }
            }

            private sealed class Worker
            {
                public BlockingCollection<DrawBatch> Queue { get; } = new();
                public Thread Thread { get; }

                public Worker(string name)
                {
                    Thread = new Thread(Run)
                    {
                        IsBackground = true,
                        Name = name
                    };
                }

                private void Run()
                {
                    using (CanvasDrawCommandQueue.EnterWorkerThread())
                    {
                        foreach (var batch in Queue.GetConsumingEnumerable())
                            batch.Execute();
                    }
                }
            }

            private sealed class DrawBatch
            {
                private readonly int _itemCount;
                private readonly Action<int> _action;
                private readonly ConcurrentQueue<Exception> _exceptions = new();
                private readonly ManualResetEventSlim _completed = new(false);
                private int _nextIndex = -1;
                private int _remainingWorkers;

                public DrawBatch(int itemCount, int workerCount, Action<int> action)
                {
                    _itemCount = itemCount;
                    _remainingWorkers = workerCount;
                    _action = action;
                }

                public void Execute()
                {
                    try
                    {
                        while (true)
                        {
                            var index = Interlocked.Increment(ref _nextIndex);
                            if (index >= _itemCount)
                                break;

                            try
                            {
                                _action(index);
                            }
                            catch (Exception exception)
                            {
                                _exceptions.Enqueue(exception);
                            }
                        }
                    }
                    finally
                    {
                        if (Interlocked.Decrement(ref _remainingWorkers) == 0)
                            _completed.Set();
                    }
                }

                public void Wait()
                {
                    _completed.Wait();
                    _completed.Dispose();
                    if (_exceptions.IsEmpty)
                        return;

                    throw new AggregateException(_exceptions);
                }
            }
        }
    }
}
