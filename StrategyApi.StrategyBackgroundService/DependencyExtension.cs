using System.Threading.Channels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StrategyApi.Mail;
using StrategyApi.StrategyBackgroundService.Dto.Command.Api;
using StrategyApi.StrategyBackgroundService.Dto.Command.Result;
using StrategyApi.StrategyBackgroundService.Dto.Command.Strategy;
using StrategyApi.StrategyBackgroundService.Services;

namespace StrategyApi.StrategyBackgroundService;

public static class DependencyExtension
{
    public static void AddDependencyBackgroundService(this IServiceCollection service, ConfigurationManager configuration)
    {
        service.AddHostedService<StrategyBackgroundService>();
        service.AddScoped<IApiConnectService, ApiConnectService>();
        service.AddScoped<IStrategyHandlerService, StrategyHandlerService>();
        var channelApi = Channel.CreateUnbounded<(ApiCommandBaseDto, TaskCompletionSource<CommandResultBase>)>();
        service.AddSingleton(channelApi.Reader);
        service.AddSingleton(channelApi.Writer);
        var channelStrategy = Channel.CreateUnbounded<(StrategyCommandBaseDto, TaskCompletionSource<CommandResultBase>)>();
        service.AddSingleton(channelStrategy.Reader);
        service.AddSingleton(channelStrategy.Writer);
        service.AddSingleton<IEmailService,EmailService>();
        service.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));

    }
}