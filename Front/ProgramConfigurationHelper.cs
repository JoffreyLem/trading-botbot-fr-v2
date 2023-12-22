using System.Threading.Channels;
using Destructurama;
using Serilog;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Exceptions;
using StrategyApi.Mail;
using StrategyApi.StrategyBackgroundService;
using StrategyApi.StrategyBackgroundService.Command.Api;
using StrategyApi.StrategyBackgroundService.Command.Strategy;
using StrategyApi.StrategyBackgroundService.Services;
using Syncfusion.Blazor;
using Syncfusion.Licensing;

namespace Front;

public static class ProgramConfigurationHelper
{
    public static void AddSyncFusion(this WebApplicationBuilder builder)
    {
        SyncfusionLicenseProvider.RegisterLicense(
            "Ngo9BigBOggjHTQxAR8/V1NAaF5cWWJCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdgWH1ccnRUQ2BZVkNzX0Q=");
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

    public static void AddBotDependency(this WebApplicationBuilder builder)
    {
        builder.Services.AddHostedService<StrategyBackgroundService>();
        builder.Services.AddSingleton<IApiConnectService, ApiConnectService>();
        builder.Services.AddSingleton<IStrategyHandlerService, StrategyHandlerService>();
        var channelApi = Channel.CreateUnbounded<ServiceCommandeBaseApiAbstract>();
        builder.Services.AddSingleton(channelApi.Reader);
        builder.Services.AddSingleton(channelApi.Writer);
        var channelStrategy =
            Channel.CreateUnbounded<ServiceCommandeBaseStrategyAbstract>();
        builder.Services.AddSingleton(channelStrategy.Reader);
        builder.Services.AddSingleton(channelStrategy.Writer);
        builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
        builder.Services.AddSingleton<IEmailService, EmailService>();
    }
}