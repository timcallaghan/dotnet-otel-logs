using Serilog;
using Serilog.Events;

namespace WebHost.Telemetry.Logging;

public static class SerilogExtensions
{
    public static LoggerConfiguration ConfigureSerilog(
            this LoggerConfiguration loggerConfiguration,
            IConfiguration configuration)
    {
        var configurationBuilder = loggerConfiguration
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Conditional(
                // For HTTP request logs, never send them to distributed tracing/activity events, because that
                // information is already there in a much nicer format.
                condition: e =>
                    e.MessageTemplate.Text !=
                    "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms",
                sink => sink.Sink<ActivityEventWriterSink>())
            .WriteTo.Seq(configuration.GetValue<string>(SeqOptions.SeqUrl));

        return configurationBuilder;
    }
    
    public static void UseSerilogRequestLogging(this IApplicationBuilder app)
    {
        if (app.ApplicationServices.GetService<Serilog.Extensions.Hosting.DiagnosticContext>() != null)
        {
            app.UseSerilogRequestLogging(serilogOptions =>
            {
                serilogOptions.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                    diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                };
            });
        }
    }
}