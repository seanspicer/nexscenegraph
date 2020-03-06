using Microsoft.Extensions.Logging;

namespace Veldrid.SceneGraph.Logging
{
    public class LogManager
    {
        private static ILoggerFactory _Factory = null;
        
        public static void SetLogger(ILoggerFactory factory)
        {
            _Factory = factory;
        }
        
        public static ILoggerFactory LoggerFactory
        {
            get
            {
                if (_Factory == null)
                {
                    var factory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
                    {
                        builder.AddFilter(level => level >= LogLevel.Trace);
                    });
                    SetLogger(factory);
                }
                return _Factory;
            }
            //set { _Factory = value; }
        }
        public static ILogger CreateLogger<T>() => LoggerFactory.CreateLogger<T>();
        public static ILogger CreateLogger(string type) => LoggerFactory.CreateLogger(type);
    }
}