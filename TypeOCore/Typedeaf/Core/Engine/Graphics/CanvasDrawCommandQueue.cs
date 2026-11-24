namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine.Graphics
    {
        /// <summary>Records canvas operations during worker-thread drawing for later graphics-thread execution.</summary>
        public static class CanvasDrawCommandQueue
        {
            [ThreadStatic]
            private static List<Action> _currentCommands;

            [ThreadStatic]
            private static bool _isWorkerThread;

            [ThreadStatic]
            private static bool _isExecutingRecordedCommands;

            [ThreadStatic]
            private static PendingBatch _pendingBatch;

            [ThreadStatic]
            private static Dictionary<Type, PendingBatch> _batchPool;

            /// <summary>Gets whether the current thread is recording drawable canvas operations.</summary>
            public static bool IsRecording => _currentCommands != null;

            /// <summary>Gets whether the current thread is an engine update or draw worker.</summary>
            public static bool IsWorkerThread => _isWorkerThread;

            /// <summary>Adds an operation to the current drawable's command list, if one is being recorded.</summary>
            public static bool TryEnqueue(Action command)
            {
                if (_currentCommands == null)
                {
                    if (_isWorkerThread)
                        throw new InvalidOperationException("Canvas operations on engine workers must be issued from a drawable Draw call.");
                    return false;
                }
                if (command == null)
                    throw new ArgumentNullException(nameof(command));

                _currentCommands.Add(command);
                return true;
            }

            /// <summary>Queues a compatible command into the current graphics-thread batch.</summary>
            /// <typeparam name="T">The type of the batched command.</typeparam>
            /// <param name="key">The shared object identifying commands that can be batched together.</param>
            /// <param name="command">The command to append.</param>
            /// <param name="flush">The action that executes the accumulated commands on the graphics thread.</param>
            /// <returns><see langword="true"/> when the command was batched during recorded-command replay.</returns>
            public static bool TryEnqueueBatch<T>(object key, T command, Action<object, IReadOnlyList<T>> flush)
            {
                if (!_isExecutingRecordedCommands)
                    return false;
                if (key == null)
                    throw new ArgumentNullException(nameof(key));
                if (flush == null)
                    throw new ArgumentNullException(nameof(flush));

                if (_pendingBatch is PendingBatch<T> current && ReferenceEquals(current.Key, key))
                {
                    current.Commands.Add(command);
                    return true;
                }

                FlushPendingBatch();
                if (_batchPool != null && _batchPool.TryGetValue(typeof(T), out var pooledBatch))
                {
                    _batchPool.Remove(typeof(T));
                    var batch = (PendingBatch<T>)pooledBatch;
                    batch.Reset(key, flush);
                    batch.Commands.Add(command);
                    _pendingBatch = batch;
                }
                else
                {
                    var batch = new PendingBatch<T>(key, flush);
                    batch.Commands.Add(command);
                    _pendingBatch = batch;
                }
                return true;
            }

            /// <summary>Flushes queued batch work before an incompatible graphics operation.</summary>
            public static void FlushPendingBatch()
            {
                var pendingBatch = _pendingBatch;
                _pendingBatch = null;
                if (pendingBatch == null)
                    return;

                try
                {
                    pendingBatch.Flush();
                }
                finally
                {
                    _batchPool ??= new Dictionary<Type, PendingBatch>();
                    _batchPool[pendingBatch.BatchType] = pendingBatch;
                }
            }

            internal static IDisposable BeginCommandReplay()
            {
                var previousValue = _isExecutingRecordedCommands;
                _isExecutingRecordedCommands = true;
                return new CommandReplayScope(previousValue);
            }

            internal static IDisposable EnterWorkerThread()
            {
                var previousValue = _isWorkerThread;
                _isWorkerThread = true;
                return new WorkerThreadScope(previousValue);
            }

            internal static IDisposable BeginRecording(List<Action> commands)
            {
                var previousCommands = _currentCommands;
                _currentCommands = commands;
                return new RecordingScope(previousCommands);
            }

            internal static void ExecuteOrEnqueue(Action command)
            {
                if (_currentCommands != null)
                    _currentCommands.Add(command);
                else
                    command();
            }

            private sealed class WorkerThreadScope : IDisposable
            {
                private readonly bool _previousValue;
                private bool _disposed;

                public WorkerThreadScope(bool previousValue)
                {
                    _previousValue = previousValue;
                }

                public void Dispose()
                {
                    if (_disposed)
                        return;

                    _isWorkerThread = _previousValue;
                    _disposed = true;
                }
            }

            private sealed class RecordingScope : IDisposable
            {
                private readonly List<Action> _previousCommands;
                private bool _disposed;

                public RecordingScope(List<Action> previousCommands)
                {
                    _previousCommands = previousCommands;
                }

                public void Dispose()
                {
                    if (_disposed)
                        return;

                    _currentCommands = _previousCommands;
                    _disposed = true;
                }
            }

            private abstract class PendingBatch
            {
                public object Key { get; protected set; }
                public abstract Type BatchType { get; }

                protected PendingBatch(object key)
                {
                    Key = key;
                }

                public abstract void Flush();
            }

            private sealed class PendingBatch<T> : PendingBatch
            {
                public List<T> Commands { get; } = new();
                private Action<object, IReadOnlyList<T>> _flush;

                public PendingBatch(object key, Action<object, IReadOnlyList<T>> flush) : base(key)
                {
                    _flush = flush;
                }

                public override Type BatchType => typeof(T);

                public void Reset(object key, Action<object, IReadOnlyList<T>> flush)
                {
                    Key = key;
                    _flush = flush;
                }

                public override void Flush()
                {
                    var key = Key;
                    var flush = _flush;
                    try
                    {
                        if (Commands.Count > 0)
                            flush(key, Commands);
                    }
                    finally
                    {
                        Commands.Clear();
                        Key = null;
                        _flush = null;
                    }
                }
            }

            private sealed class CommandReplayScope : IDisposable
            {
                private readonly bool _previousValue;
                private bool _disposed;

                public CommandReplayScope(bool previousValue)
                {
                    _previousValue = previousValue;
                }

                public void Dispose()
                {
                    if (_disposed)
                        return;

                    try
                    {
                        if (!_previousValue)
                            FlushPendingBatch();
                    }
                    finally
                    {
                        _isExecutingRecordedCommands = _previousValue;
                        _disposed = true;
                    }
                }
            }
        }
    }
}
