
using TypeOEngine.Typedeaf.Core.Engine.Interfaces;

namespace TypeOEngine.Typedeaf.Core.Engine
{
    public abstract class Logger : TypeOObject, ILogger
    {
        public abstract void Log(LogLevel level, string log);

        /// <summary>
        /// Log a message with Info log level severity.
        /// </summary>
        /// <param name="log">Message to log.</param>
        public void Log(string log)
        {
            Log(LogLevel.Info, log);
        }

        public abstract void SetOption(ILoggerOption option);
    }
}
