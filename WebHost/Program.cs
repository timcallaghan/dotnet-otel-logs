using System.Reflection;
using OpenTelemetry.Resources;
using Serilog;
using WebHost.Telemetry;
using WebHost.Telemetry.Logging;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
    var serviceName = builder.Configuration.GetValue<string>(OtlpOptions.ServiceName);

    var resourceBuilder = ResourceBuilder.CreateDefault()
        .AddService(serviceName, serviceVersion: assemblyVersion, serviceInstanceId: Environment.MachineName);

    builder.Services.AddOtelTracing(builder.Configuration, resourceBuilder);
    builder.Services.AddOtelMetrics(builder.Configuration, resourceBuilder);
    builder.Logging.AddOtelLogging(builder.Configuration, resourceBuilder, false);

    builder.Host.UseSerilog((hostingContext, services, loggerConfiguration) =>
    {
        loggerConfiguration.ConfigureSerilog(builder.Configuration);
    }, writeToProviders: true);

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseSerilogRequestLogging();
    
    app.UseAuthorization();

    app.MapControllers();
    
    await app.RunAsync();
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
    return 1;
}
finally
{
    Log.Information("Shut down");
    Log.CloseAndFlush();
}