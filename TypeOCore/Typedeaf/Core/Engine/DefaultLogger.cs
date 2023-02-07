using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TypeOEngine.Typedeaf.Core.Engine.Interfaces;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine
    {
        /// <summary>
        /// Default Logger instance, will write to System.Console.WriteLine and System.Diagnostics.Debug.WriteLine. If SaveLogs option is set to True, then will also write to file on disk.
        /// </summary>
        public class DefaultLogger : ILogger, IHasContext
        {
            private static readonly Mutex FileAccessMutex = new Mutex();

            Context IHasContext.Context { get; set; }
            private Context Context { get => (this as IHasContext).Context; set => (this as IHasContext).Context = value; }

            private bool SaveLogs { get; set; }
            private string LogPath { get; set; }
            /// <summary>
            /// Current Log level
            /// </summary>
            public LogLevel LogLevel { get; private set; }

            private List<string> Logs { get; set; }

            private DateTime LastTick { get; set; }
            private double TimeToLog { get; set; } = 0;
            private double TimerToLog { get; set; } = 1;

            /// <summary>
            /// Do not instanciate, instead set through TypeO Context object by using SetLogger.
            /// </summary>
            public DefaultLogger()
            {
                Logs = new List<string>();
            }
            
            /// <inheritdoc/>
            public void SetOption(ILoggerOption option)
            {
                LogLevel = option.LogLevel;
                if(option is DefaultLoggerOption defaultLoggerOption)
                {
                    SaveLogs = defaultLoggerOption.SaveLogs;
                    LogPath = defaultLoggerOption.LogPath;
                }
            }

            /// <inheritdoc/>
            public async void Log(LogLevel level, string log)
            {
                if(LogLevel == LogLevel.None) return;
                if(LogLevel > level) return;

                var defaultColor = Console.ForegroundColor;
                string levelMessage = "";
                switch(level)
                {
                    case LogLevel.Ludacris:
                        levelMessage = "LDCR";
                        break;
                    case LogLevel.Debug:
                        levelMessage = "DBUG";
                        break;
                    case LogLevel.Info:
                        levelMessage = "INFO";
                        break;
                    case LogLevel.Warning:
                        levelMessage = "WARN";
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case LogLevel.Error:
                        levelMessage = "EROR";
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case LogLevel.Fatal:
                        levelMessage = "FATL";
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    default:
                        break;
                }

                var logMessage = $"{Context.Name} - {levelMessage}: {log}";
                Debug.WriteLine(logMessage);
                Console.WriteLine(logMessage);

                FileAccessMutex.WaitOne();
                Logs.Add(logMessage);
                FileAccessMutex.ReleaseMutex();

                Console.ForegroundColor = defaultColor;

                if(SaveLogs)
                {
                    var now = DateTime.UtcNow;
                    TimeToLog += (now - LastTick).TotalSeconds;
                    LastTick = now;
                    if(TimeToLog >= TimerToLog)
                    {
                        TimeToLog = 0;
                        await WriteLogsToDisk();
                    }
                }
            }

            private async Task WriteLogsToDisk()
            {
                await Task.Run(() =>
                {
                    FileAccessMutex.WaitOne();
                    try
                    {
                        var dirPath = string.IsNullOrEmpty(LogPath) ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TypeO", "Logs", Context.Name) : LogPath;
                        var filePath = $"{DateTime.UtcNow.ToShortDateString()}.log".Replace("/", "-");
                        var logPath = Path.Combine(dirPath, filePath);

                        if(!Directory.Exists(dirPath))
                        {
                            Directory.CreateDirectory(dirPath);
                        }

                        using(var logFile = File.AppendText(logPath))
                        {
                            foreach(var logMessage in Logs)
                            {
                                logFile.WriteLine(logMessage);
                            }
                        }

                        Logs.Clear();
                    }
                    catch
                    {
                        FileAccessMutex.ReleaseMutex();
                        throw;
                    }
                    FileAccessMutex.ReleaseMutex();
                });
            }

            public void Cleanup()
            {
                _ = WriteLogsToDisk();
            }
        }
    }
}
