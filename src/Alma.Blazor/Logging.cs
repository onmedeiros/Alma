using Serilog;

namespace Alma.Blazor
{
    public static class Logging
    {
        public static string LogOutputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";

        public static Serilog.ILogger ConfigureLogger()
        {
            return new LoggerConfiguration()
                 .WriteTo.Console(outputTemplate: LogOutputTemplate)
                 .Enrich.FromLogContext()
                 .CreateBootstrapLogger();
        }
    }
}