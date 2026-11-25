using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using TypeOEngine.Typedeaf.Core.Engine.Interfaces;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine
    {
        /// <summary>
        /// Default Logger instance, which writes output and optional log files on a background thread.
        /// </summary>
        public class DefaultLogger : Logger, IHasContext
        {
            private const int MaxBatchSize = 4096;
            private const int BatchDelayMilliseconds = 20;
            private static readonly object ConsoleLock = new();
            private static readonly object FileAccessLock = new();

            private readonly ConcurrentQueue<LogEntry> _pendingLogs = new();
            private readonly AutoResetEvent _queueChanged = new(false);
            private readonly object _lifecycleLock = new();
            private readonly Thread _writerThread;
            private bool _stopping;
            private bool _cleanupStarted;
            private bool _queueDisposed;
            private int _signalPending;
            private ExceptionDispatchInfo _writerException;

            Context IHasContext.Context { get; set; }
            private Context Context { get => (this as IHasContext).Context; set => (this as IHasContext).Context = value; }

            private bool ThrowExceptionOnFatal { get; set; }
            private bool SaveLogs { get; set; }
            private string LogPath { get; set; }

            /// <summary>Gets the current log level.</summary>
            public LogLevel LogLevel { get; private set; }

            /// <summary>
            /// A log message passed to the background batch processor.
            /// </summary>
            protected readonly struct LogEntry
            {
                /// <summary>Gets the message severity.</summary>
                public LogLevel Level { get; }
                /// <summary>Gets the unformatted message text.</summary>
                public string Message { get; }
                /// <summary>Gets whether the default console and file sinks accept this message.</summary>
                public bool WriteToDefaultOutputs { get; }

                internal LogEntry(LogLevel level, string message, bool writeToDefaultOutputs)
                {
                    Level = level;
                    Message = message;
                    WriteToDefaultOutputs = writeToDefaultOutputs;
                }
            }

            /// <summary>
            /// Do not instantiate directly; configure through the TypeO context using SetLogger.
            /// </summary>
            public DefaultLogger()
            {
                _writerThread = new Thread(WriterThread)
                {
                    IsBackground = true,
                    Name = "TypeO log writer"
                };
                _writerThread.Start();
            }

            /// <summary>Initializes the logger.</summary>
            protected override void Initialize()
            {
            }

            /// <summary>Flushes queued messages and stops the background writer.</summary>
            protected override void Cleanup()
            {
                lock (_lifecycleLock)
                {
                    if (!_cleanupStarted)
                    {
                        _cleanupStarted = true;
                        _stopping = true;
                        SignalWriter();
                    }
                }
                try
                {
                    if (Thread.CurrentThread != _writerThread)
                        _writerThread.Join();

                    ThrowIfWriterFailed();
                }
                finally
                {
                    lock (_lifecycleLock)
                    {
                        if (!_queueDisposed)
                        {
                            _queueChanged.Dispose();
                            _queueDisposed = true;
                        }
                    }
                }
            }

            /// <inheritdoc/>
            public override void SetOption(ILoggerOption option)
            {
                LogLevel = option.LogLevel;
                if(option is DefaultLoggerOption defaultLoggerOption)
                {
                    SaveLogs = defaultLoggerOption.SaveLogs;
                    LogPath = defaultLoggerOption.LogPath;
                    ThrowExceptionOnFatal = defaultLoggerOption.ThrowExceptionOnFatal;
                }
            }

            /// <summary>Writes accepted log messages to the background queue.</summary>
            public override void Log(LogLevel level, string log)
            {
                var writeToDefaultOutputs = LogLevel != LogLevel.None && LogLevel <= level;
                if (!writeToDefaultOutputs && !ShouldQueueFilteredMessage(level))
                {
                    FatalExceptionThrow(level, log);
                    return;
                }

                lock (_lifecycleLock)
                {
                    ThrowIfWriterFailed();
                    if (_stopping)
                        throw new ObjectDisposedException(nameof(DefaultLogger));

                    _pendingLogs.Enqueue(new LogEntry(level, log, writeToDefaultOutputs));
                    SignalWriter();
                }
                FatalExceptionThrow(level, log);
            }

            /// <summary>Allows derived loggers to queue messages hidden by the default output level.</summary>
            protected virtual bool ShouldQueueFilteredMessage(LogLevel level)
            {
                return false;
            }

            /// <summary>Processes the editor or application-specific portion of a drained batch.</summary>
            protected virtual void ProcessLogBatch(IReadOnlyList<LogEntry> batch)
            {
            }

            private void WriterThread()
            {
                var batch = new List<LogEntry>(MaxBatchSize);
                try
                {
                    while (true)
                    {
                        _queueChanged.WaitOne();
                        Interlocked.Exchange(ref _signalPending, 0);

                        bool stopping;
                        lock (_lifecycleLock)
                            stopping = _stopping;
                        if (!stopping)
                            Thread.Sleep(BatchDelayMilliseconds);

                        batch.Clear();
                        while (batch.Count < MaxBatchSize && _pendingLogs.TryDequeue(out var entry))
                            batch.Add(entry);

                        if (batch.Count > 0)
                            WriteBatch(batch);

                        lock (_lifecycleLock)
                        {
                            if (_stopping && _pendingLogs.IsEmpty)
                                return;
                        }

                        if (!_pendingLogs.IsEmpty)
                            SignalWriter();
                    }
                }
                catch (Exception exception)
                {
                    Interlocked.CompareExchange(
                        ref _writerException,
                        ExceptionDispatchInfo.Capture(exception),
                        null);
                    lock (_lifecycleLock)
                        _stopping = true;
                    SignalWriter();
                }
            }

            private void SignalWriter()
            {
                if (Interlocked.Exchange(ref _signalPending, 1) == 0)
                    _queueChanged.Set();
            }

            private void WriteBatch(IReadOnlyList<LogEntry> batch)
            {
                ProcessLogBatch(batch);

                var hasDefaultOutput = false;
                foreach (var entry in batch)
                {
                    if (entry.WriteToDefaultOutputs)
                    {
                        hasDefaultOutput = true;
                        break;
                    }
                }

                if (!hasDefaultOutput)
                    return;

                StreamWriter logFile = null;
                if (SaveLogs)
                {
                    var directoryPath = string.IsNullOrEmpty(LogPath)
                        ? Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            "TypeO",
                            "Logs",
                            Context.Name)
                        : LogPath;
                    Directory.CreateDirectory(directoryPath);
                    var fileName = $"{DateTime.UtcNow.ToShortDateString()}.log".Replace("/", "-");
                    var filePath = Path.Combine(directoryPath, fileName);
                    Monitor.Enter(FileAccessLock);
                    try
                    {
                        logFile = File.AppendText(filePath);
                    }
                    catch
                    {
                        Monitor.Exit(FileAccessLock);
                        throw;
                    }
                }

                try
                {
                    foreach (var entry in batch)
                    {
                        if (!entry.WriteToDefaultOutputs)
                            continue;

                        var logMessage = FormatLogMessage(entry);
                        logFile?.WriteLine(logMessage);
                        WriteToConsole(entry.Level, logMessage);
                    }
                }
                finally
                {
                    if (logFile != null)
                    {
                        try
                        {
                            logFile.Dispose();
                        }
                        finally
                        {
                            Monitor.Exit(FileAccessLock);
                        }
                    }
                }
            }

            private string FormatLogMessage(LogEntry entry)
            {
                var levelMessage = entry.Level switch
                {
                    LogLevel.Ludacris => "LDCR",
                    LogLevel.Debug => "DBUG",
                    LogLevel.Info => "INFO",
                    LogLevel.Warning => "WARN",
                    LogLevel.Error => "EROR",
                    LogLevel.Fatal => "FATL",
                    _ => ""
                };
                return $"{Context.Name} - {levelMessage}: {entry.Message}";
            }

            /// <summary>Writes one formatted message to the console sink.</summary>
            protected virtual void WriteToConsole(LogLevel level, string message)
            {
                lock (ConsoleLock)
                {
                    var originalColor = Console.ForegroundColor;
                    try
                    {
                        if (level == LogLevel.Warning)
                            Console.ForegroundColor = ConsoleColor.Yellow;
                        else if (level == LogLevel.Error || level == LogLevel.Fatal)
                            Console.ForegroundColor = ConsoleColor.Red;

                        Console.WriteLine(message);
                    }
                    finally
                    {
                        Console.ForegroundColor = originalColor;
                    }
                }
            }

            private void ThrowIfWriterFailed()
            {
                Volatile.Read(ref _writerException)?.Throw();
            }

            private void FatalExceptionThrow(LogLevel level, string log)
            {
                if (ThrowExceptionOnFatal && level == LogLevel.Fatal)
                    throw new Exception(log);
            }
        }
    }
}
