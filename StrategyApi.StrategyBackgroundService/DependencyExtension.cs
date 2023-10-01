using System.Threading.Channels;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using StrategyApi.Mail;
using StrategyApi.StrategyBackgroundService.Dto.Command.Api;
using StrategyApi.StrategyBackgroundService.Dto.Command.Result;
using StrategyApi.StrategyBackgroundService.Dto.Command.Strategy;
using StrategyApi.StrategyBackgroundService.Hubs;
using StrategyApi.StrategyBackgroundService.Mapper;
using StrategyApi.StrategyBackgroundService.Services;

namespace StrategyApi.StrategyBackgroundService;

public static class DependencyExtension
{
    public static void AddDependencyRobot(this WebApplicationBuilder builder)
    {
        builder.Services.AddHostedService<StrategyBackgroundService>();
        builder.Services.AddScoped<IApiConnectService, ApiConnectService>();
        builder.Services.AddScoped<IStrategyHandlerService, StrategyHandlerService>();
        var channelApi = Channel.CreateUnbounded<(ApiCommandBaseDto, TaskCompletionSource<CommandResultBase>)>();
        builder.Services.AddSingleton(channelApi.Reader);
        builder.Services.AddSingleton(channelApi.Writer);
        var channelStrategy =
            Channel.CreateUnbounded<(StrategyCommandBaseDto, TaskCompletionSource<CommandResultBase>)>();
        builder.Services.AddSingleton(channelStrategy.Reader);
        builder.Services.AddSingleton(channelStrategy.Writer);
        builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
        builder.Services.AddSingleton<IEmailService, EmailService>();

        builder.Services.AddAutoMapper(cfg => { cfg.AddProfile<MappingProfilesBackgroundServices>(); },
            typeof(MappingProfilesBackgroundServices).Assembly
        );
    }

    public static void AddDependency2(this WebApplication app)
    {
        app.MapHub<StrategyHub>(StrategyHub.HubName);
        app.MapHub<ApiHandlerHub>(ApiHandlerHub.ApiHubName);
    }
}