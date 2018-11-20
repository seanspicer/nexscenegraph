using System;

namespace Veldrid.SceneGraph.Logging
{
    internal class NoOpLogger : IVeldridSceneGraphLogger
    {
        public void Info(Func<string> log) {}
        
        public void Debug(Func<string> log) {}

        public void Warn(Func<string> log) {}

        public void Error(Func<string> log) {}

        public void Fatal(Func<string> log) {}

        public void Verbose(Func<string> log) {}
    }
    
    public class LoggingService : IVeldridSceneGraphLoggingService
    {
        private IVeldridSceneGraphLogger _logger;
        
        private static readonly Lazy<IVeldridSceneGraphLoggingService> lazy = 
            new Lazy<IVeldridSceneGraphLoggingService>(() => new LoggingService());

        public static IVeldridSceneGraphLoggingService Instance => lazy.Value;

        private LoggingService()
        {
            _logger = new NoOpLogger();
        }
        
        public IVeldridSceneGraphLogger GetLogger()
        {
            return _logger;
        }

        public void RegisterLogger(IVeldridSceneGraphLogger logger)
        {
            _logger = logger;
        }
    }
}