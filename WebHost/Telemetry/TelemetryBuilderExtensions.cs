using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace WebHost.Telemetry;

public static class TelemetryBuilderExtensions
{
    public static IServiceCollection AddOtelTracing(
        this IServiceCollection services, 
        IConfiguration configuration,
        ResourceBuilder resourceBuilder)
    {
        services.AddOpenTelemetryTracing(options =>
        {
            options.SetResourceBuilder(resourceBuilder)
                .SetSampler(new AlwaysOnSampler())
                .AddAspNetCoreInstrumentation();
    
            options.AddOtlpExporter(otlpOptions =>
            {
                otlpOptions.Endpoint = new Uri(configuration.GetValue<string>(OtlpOptions.Endpoint));
            });
        });

        services.Configure<AspNetCoreInstrumentationOptions>(configuration.GetSection(InstrumentationOptions.AspNetCoreInstrumentationSectionName));
        
        return services;
    }
    
    public static ILoggingBuilder AddOtelLogging(
        this ILoggingBuilder loggingBuilder, 
        IConfiguration configuration,
        ResourceBuilder resourceBuilder,
        bool clearProviders = true)
    {
        if (clearProviders)
        {
            loggingBuilder.ClearProviders();
        }
        
        loggingBuilder.AddOpenTelemetry(options =>
        {
            options.IncludeFormattedMessage = true;
            options.IncludeScopes = true;
            options.ParseStateValues = true;
            options.SetResourceBuilder(resourceBuilder);
            options.AddOtlpExporter(otlpOptions =>
            {
                otlpOptions.Endpoint = new Uri(configuration.GetValue<string>(OtlpOptions.Endpoint));
            });
        });
        
        return loggingBuilder;
    }
    
    public static IServiceCollection AddOtelMetrics(
        this IServiceCollection services, 
        IConfiguration configuration,
        ResourceBuilder resourceBuilder)
    {
        services.AddOpenTelemetryMetrics(options =>
        {
            options.SetResourceBuilder(resourceBuilder)
                .AddAspNetCoreInstrumentation();
    
            options.AddOtlpExporter(otlpOptions =>
            {
                otlpOptions.Endpoint = new Uri(configuration.GetValue<string>(OtlpOptions.Endpoint));
            });
        });

        return services;
    }
}