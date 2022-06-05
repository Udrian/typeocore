namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine.Interfaces
    {
        public interface ILogger
        {
            public LogLevel LogLevel { get; set; }

            public void Log(LogLevel level, string log);
            public void Log(string log)
            {
                Log(LogLevel.Info, log);
            }

            public void Cleanup();
        }
    }
}
