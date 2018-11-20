using Common.Logging;
using Common.Logging.Serilog;
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
                .WriteTo.ColoredConsole(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm::ss} [{Level}]: {Message}{NewLine}")
                .CreateLogger();

            LogManager.Adapter = new SerilogFactoryAdapter(Log.Logger);
            
        }
    }
}