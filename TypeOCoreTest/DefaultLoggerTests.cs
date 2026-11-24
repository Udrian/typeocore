using TypeOEngine.Typedeaf.Core;
using TypeOEngine.Typedeaf.Core.Engine;
using TypeOEngine.Typedeaf.Core.Engine.Interfaces;

namespace TypeOCoreTest;

public class DefaultLoggerTests
{
    private const int MessageCount = 20_000;
    private const string MessagePrefix = "logger-batch-test-";

    public sealed class FileOnlyLogger : DefaultLogger
    {
        public int LoggedBatchCount;
        public int LoggedMessageCount;
        public int LogWriterThreadId;

        protected override void ProcessLogBatch(IReadOnlyList<LogEntry> batch)
        {
            var messageCount = 0;
            foreach (var entry in batch)
            {
                if (entry.Message?.StartsWith(MessagePrefix, StringComparison.Ordinal) == true)
                    messageCount++;
            }

            if (messageCount > 0)
            {
                Interlocked.Increment(ref LoggedBatchCount);
                Interlocked.Add(ref LoggedMessageCount, messageCount);
                Volatile.Write(ref LogWriterThreadId, Environment.CurrentManagedThreadId);
            }
        }

        protected override void WriteToConsole(LogLevel level, string message)
        {
        }
    }

    public sealed class LoggingGame : Game
    {
        public ILogger Logger { get; set; }
        public int UpdateThreadId { get; private set; }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void Cleanup()
        {
        }

        public override void Update(double dt)
        {
            UpdateThreadId = Environment.CurrentManagedThreadId;
            for (var i = 0; i < MessageCount; i++)
                Logger.Log(LogLevel.Debug, $"{MessagePrefix}{i}");
            Exit();
        }

        public override void Draw()
        {
        }
    }

    [Fact]
    public void QueuedMessagesAreWrittenBeforeLoggerCleanupReturns()
    {
        var logDirectory = Path.Combine(Path.GetTempPath(), $"TypeO-logger-{Guid.NewGuid():N}");
        try
        {
            var typeO = Assert.IsType<TypeO>(
                TypeO.Create<LoggingGame>("logger-test")
                    .SetLogger<FileOnlyLogger>(new DefaultLoggerOption
                    {
                        LogLevel = LogLevel.Debug,
                        SaveLogs = true,
                        LogPath = logDirectory
                    }));

            typeO.Start();
            var logger = Assert.IsType<FileOnlyLogger>(typeO.Context.Logger);
            var game = Assert.IsType<LoggingGame>(typeO.Context.Game);

            var matchingLines = Directory.GetFiles(logDirectory, "*.log")
                .SelectMany(File.ReadLines)
                .Where(line => line.Contains(MessagePrefix, StringComparison.Ordinal))
                .ToArray();

            Assert.Equal(MessageCount, matchingLines.Length);
            Assert.Equal(MessageCount, logger.LoggedMessageCount);
            Assert.InRange(logger.LoggedBatchCount, 1, MessageCount - 1);
            Assert.NotEqual(game.UpdateThreadId, logger.LogWriterThreadId);
            Assert.Contains($"{MessagePrefix}0", matchingLines[0], StringComparison.Ordinal);
            Assert.Contains($"{MessagePrefix}{MessageCount - 1}", matchingLines[^1], StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(logDirectory))
                Directory.Delete(logDirectory, recursive: true);
        }
    }
}
