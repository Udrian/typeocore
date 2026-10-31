
using TypeOEngine.Typedeaf.Core.Engine.Interfaces;

namespace TypeOEngine.Typedeaf.Core.Engine
{
    /// <summary>Defines the base behavior for TypeO loggers.</summary>
    public abstract class Logger : TypeOObject, ILogger
    {
        /// <summary>Writes a message with the specified severity.</summary>
        public abstract void Log(LogLevel level, string log);

        /// <summary>
        /// Log a message with Info log level severity.
        /// </summary>
        /// <param name="log">Message to log.</param>
        public void Log(string log)
        {
            Log(LogLevel.Info, log);
        }

        /// <summary>Sets the options used to configure this logger.</summary>
        public abstract void SetOption(ILoggerOption option);
    }
}
