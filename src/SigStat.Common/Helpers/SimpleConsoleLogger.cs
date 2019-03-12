﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SigStat.Common.Helpers
{
    /// <summary>
    /// A easy-to-use class to log pipeline messages, complete with filtering levels and multi-thread support.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>A producer-consumer pattern is implemented with a concurrent queue to support multi-threaded pipelines.</item>
    /// <item>Holding the StreamWriter open is more efficient than repeatedly opening and closing it.</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// Logger l1 = new Logger(LogLevel.Info);
    /// Logger.Warn(this, "Training on non-genuine signature.");
    /// </code>
    /// </example>
    public class SimpleConsoleLogger : ILogger
    {
        /// <summary>
        /// All events below this level will be filtered
        /// </summary>
        public LogLevel LogLevel { get; set; }

        public event EventHandler<string> Logged;

        /// <summary>
        /// Initializes a SimpleConsoleLogger instance with LogLevel set to LogLevel.Information
        /// </summary>
        public SimpleConsoleLogger(): this(LogLevel.Information)
        {

        }

        /// <summary>
        /// Initializes an instance of SimpleConsoleLogger with a custom LogLevel
        /// </summary>
        /// <param name="logLevel">initial value for LogLevel</param>
        public SimpleConsoleLogger(LogLevel logLevel)
        {
            LogLevel = logLevel;
        }
        /// <inheritdoc/>
        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotSupportedException("Scopes are not supported by SimpleConsoleLogger");
        }

        /// <inheritdoc/>

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= LogLevel;
        }

        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var oldColor = Console.ForegroundColor;


            switch (logLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    break;
                case LogLevel.Information:
                case LogLevel.None:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                default:
                    break;
            }
            string msg = formatter(state, exception);
            Console.WriteLine(msg);
            Logged?.Invoke(this, msg);
            Console.ForegroundColor = oldColor;
        }
    }

}
