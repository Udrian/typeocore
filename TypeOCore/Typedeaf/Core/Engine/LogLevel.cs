namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine
    {
        /// <summary>Specifies the severity of a log message.</summary>
        public enum LogLevel
        {
            /// <summary>Disables logging.</summary>
            None     = 0,
            /// <summary>Detailed diagnostic output.</summary>
            Ludacris = 1, 
            /// <summary>Debugging information.</summary>
            Debug    = 2,
            /// <summary>General informational messages.</summary>
            Info     = 3,
            /// <summary>Potential problems that do not stop execution.</summary>
            Warning  = 4,
            /// <summary>Errors that affect an operation.</summary>
            Error    = 5,
            /// <summary>A fatal error that may prevent continued execution.</summary>
            Fatal    = 6
        }
    }
}
