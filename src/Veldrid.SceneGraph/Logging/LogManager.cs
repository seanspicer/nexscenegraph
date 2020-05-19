using Microsoft.Extensions.Logging;

namespace Veldrid.SceneGraph.Logging
{
    public class LogManager
    {
        private static ILoggerFactory _Factory = null;

        public static void ConfigureLogger(ILoggerFactory factory)
        {
        }

        public static ILoggerFactory LoggerFactory
        {
            get
            {
                if (_Factory == null)
                {
                    // TODO - give apps some way to customize this...
                    _Factory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
                    {
                        builder.AddConsole();
                        builder.AddFilter(level => level >= LogLevel.Trace);
                    });
                    ConfigureLogger(_Factory);
                }
                return _Factory;
            }
            set { _Factory = value; }
        }
        public static ILogger CreateLogger<T>() => LoggerFactory.CreateLogger<T>();
    }
}