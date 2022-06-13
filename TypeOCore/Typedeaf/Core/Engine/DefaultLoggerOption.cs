using TypeOEngine.Typedeaf.Core.Engine.Interfaces;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine
    {
        /// <summary>
        /// Default logger options for DefaultLogger.
        /// </summary>
        public class DefaultLoggerOption : ILoggerOption
        {
            /// <inheritdoc/>
            public LogLevel LogLevel { get; set; }

            /// <summary>
            /// Saves the outputed logs to disk, defaults to False.
            /// </summary>
            public bool SaveLogs { get; set; } = false;

            /// <summary>
            /// Path to where the logs should be saved to disk, only applicable if SaveLogs is set to True.
            /// </summary>
            public string LogPath { get; set; } = null;
        }
    }
}
