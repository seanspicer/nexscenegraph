using System;
using Serilog;
using Serilog.Events;
using Veldrid.SceneGraph.Logging;

namespace Examples.Common
{
    public class Logger : IVeldridSceneGraphLogger
    {
        private ILogger _logger;
        
        public Logger(ILogger logger)
        {
            _logger = logger;
        }
        
        public void Info(Func<string> buildLogMessage)
        {
            if (_logger.IsEnabled(LogEventLevel.Information))
            {
                _logger.Information(buildLogMessage());
            }
        }

        public void Debug(Func<string> buildLogMessage)
        {
            if (_logger.IsEnabled(LogEventLevel.Debug))
            {
                _logger.Debug(buildLogMessage());
            }
        }

        public void Warn(Func<string> buildLogMessage)
        {
            if (_logger.IsEnabled(LogEventLevel.Warning))
            {
                _logger.Warning(buildLogMessage());
            }
        }

        public void Error(Func<string> buildLogMessage)
        {
            if (_logger.IsEnabled(LogEventLevel.Error))
            {
                _logger.Error(buildLogMessage());
            }
        }

        public void Fatal(Func<string> buildLogMessage)
        {
            if (_logger.IsEnabled(LogEventLevel.Fatal))
            {
                _logger.Fatal(buildLogMessage());
            }
        }

        public void Verbose(Func<string> buildLogMessage)
        {
            if (_logger.IsEnabled(LogEventLevel.Verbose))
            {
                _logger.Verbose(buildLogMessage());
            }
        }
    }
}