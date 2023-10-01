using System.Threading.Channels;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using RobotAppLibraryV2.Interfaces;
using Serilog;
using StrategyApi.Mail;
using StrategyApi.StrategyBackgroundService.Dto.Command.Api;
using StrategyApi.StrategyBackgroundService.Dto.Command.Result;
using StrategyApi.StrategyBackgroundService.Dto.Command.Strategy;
using StrategyApi.StrategyBackgroundService.Hubs;

namespace StrategyApi.StrategyBackgroundService;

public class StrategyBackgroundService : BackgroundService
{
    private readonly CommandHandler _apiHandlerGestion;
    private readonly ChannelReader<(ApiCommandBaseDto, TaskCompletionSource<CommandResultBase>)> _channelApiReader;

    private readonly ChannelReader<(StrategyCommandBaseDto, TaskCompletionSource<CommandResultBase>)>
        _channelStrategyReader;

    private readonly ILogger _logger;

    public StrategyBackgroundService(ILogger logger,
        ChannelReader<(ApiCommandBaseDto, TaskCompletionSource<CommandResultBase>)> channelApiReader,
        ChannelReader<(StrategyCommandBaseDto, TaskCompletionSource<CommandResultBase>)> channelStrategyReader,
        IHubContext<ApiHandlerHub, IApiHandlerHub> apiHandlerHub, IMapper mapper,
        IHubContext<StrategyHub, IStrategyHub> strategyHub,
        IEmailService emailService)
    {
        _logger = logger;
        _channelApiReader = channelApiReader;
        _channelStrategyReader = channelStrategyReader;
        _apiHandlerGestion = new CommandHandler(logger, apiHandlerHub, mapper, strategyHub, emailService);
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var apiTask = ProcessApiChannel(stoppingToken);
        var strategyTask = ProcessStrategyChannel(stoppingToken);

        await Task.WhenAll(apiTask, strategyTask);
    }

    private async Task ProcessApiChannel(CancellationToken stoppingToken)
    {
        await foreach (var (command, tcs) in _channelApiReader.ReadAllAsync(stoppingToken))
            try
            {
                await _apiHandlerGestion.HandleApiCommand(command, tcs);
            }
            catch (System.Exception ex)
            {
                tcs.SetException(ex);
            }
    }

    private async Task ProcessStrategyChannel(CancellationToken stoppingToken)
    {
        await foreach (var (command, tcs) in _channelStrategyReader.ReadAllAsync(stoppingToken))
            try
            {
                await _apiHandlerGestion.HandleStrategyCommand(command, tcs);
            }
            catch (System.Exception ex)
            {
                tcs.SetException(ex);
            }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _apiHandlerGestion.Shutdown();
    }
}