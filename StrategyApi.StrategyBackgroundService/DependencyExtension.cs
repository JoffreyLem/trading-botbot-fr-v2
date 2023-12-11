using System.Threading.Channels;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using StrategyApi.Mail;
using StrategyApi.StrategyBackgroundService.Command.Api;
using StrategyApi.StrategyBackgroundService.Command.Strategy;
using StrategyApi.StrategyBackgroundService.Hubs;
using StrategyApi.StrategyBackgroundService.Mapper;
using StrategyApi.StrategyBackgroundService.Services;

namespace StrategyApi.StrategyBackgroundService;

public static class DependencyExtension
{
    public static void AddDependencyRobot(this WebApplicationBuilder builder)
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

        builder.Services.AddAutoMapper(cfg => { cfg.AddProfile<MappingProfilesBackgroundServices>(); },
            typeof(MappingProfilesBackgroundServices).Assembly
        );

        builder.Services.AddSignalR();
    }

    public static void AddDependency2(this WebApplication app)
    {
        app.MapHub<StrategyHub>(StrategyHub.HubName);
        app.MapHub<ApiHandlerHub>(ApiHandlerHub.ApiHubName);
    }
}