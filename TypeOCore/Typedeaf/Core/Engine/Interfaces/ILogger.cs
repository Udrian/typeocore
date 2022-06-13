namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine.Interfaces
    {
        /// <summary>
        /// Interface for Logger implementation.
        /// </summary>
        public interface ILogger
        {
            /// <summary>
            /// Log a message with supplied log level severity.
            /// </summary>
            /// <param name="level">Severity of log.</param>
            /// <param name="log">Message to log.</param>
            public void Log(LogLevel level, string log);

            /// <summary>
            /// Log a message with Info log level severity.
            /// </summary>
            /// <param name="log">Message to log.</param>
            public void Log(string log)
            {
                Log(LogLevel.Info, log);
            }

            /// <summary>
            /// Sets the Logger options
            /// </summary>
            /// <param name="option">Option instance to set to the Logger</param>
            public void SetOption(ILoggerOption option);

            public void Cleanup();
        }
    }
}
