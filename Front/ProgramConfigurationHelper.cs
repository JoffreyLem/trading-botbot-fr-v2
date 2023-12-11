using Destructurama;
using Serilog;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Exceptions;
using StrategyApi.StrategyBackgroundService;
using Syncfusion.Blazor;
using Syncfusion.Licensing;

namespace Front;

public static class ProgramConfigurationHelper
{
    public static void AddSyncFusion(this WebApplicationBuilder builder)
    {
        SyncfusionLicenseProvider.RegisterLicense(
            "Ngo9BigBOggjHTQxAR8/V1NHaF5cXmtCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdgWXZceHVURmFfVkNwWEI=");
        builder.Services.AddSyncfusionBlazor();
    }

    public static void AddLogger(this WebApplicationBuilder builder)
    {
        var loggerConfig = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.WithExceptionDetails()
            .Destructure.UsingAttributes()
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("ApplicationName", "Robot-API")
            .Enrich.With(new RemovePropertiesEnricher())
            .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
            .MinimumLevel.Override("System", LogEventLevel.Error)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .WriteTo.Console();


        var logger = loggerConfig.CreateLogger();
        SelfLog.Enable(Console.Error);
        builder.Host.UseSerilog(logger);
        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(logger);
    }
}