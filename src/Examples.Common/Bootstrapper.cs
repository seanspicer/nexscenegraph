using Serilog;
using SharpDX.Win32;

namespace Examples.Common
{
    public static class Bootstrapper
    {
        public static void Configure()
        {
            BuildLogger();
        }

        private static void BuildLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Information()
                .WriteTo.ColoredConsole(outputTemplate: "{Timestamp:HH:mm} [{Level}] [{Source}] : {Message}{NewLine}")
                .CreateLogger()
                .ForContext("Source", "Application");
            
            Veldrid.SceneGraph.Logging.LoggingService.Instance.RegisterLogger(
                new Logger(Log.Logger.ForContext("Source", "Veldrid.SceneGraph")));
        }
    }
}