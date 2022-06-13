namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine.Interfaces
    {
        /// <summary>
        /// Interface for defining Logger options.
        /// </summary>
        public interface ILoggerOption
        {
            /// <summary>
            /// Log level severity filtering.
            /// </summary>
            public LogLevel LogLevel { get; set; }
        }
    }
}
