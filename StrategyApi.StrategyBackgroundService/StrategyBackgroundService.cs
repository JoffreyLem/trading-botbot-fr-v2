using System.Threading.Channels;
using AutoMapper;
using Microsoft.Extensions.Hosting;
using Serilog;
using StrategyApi.Mail;
using StrategyApi.StrategyBackgroundService.Command.Api;
using StrategyApi.StrategyBackgroundService.Command.Strategy;

namespace StrategyApi.StrategyBackgroundService;

public class StrategyBackgroundService : BackgroundService
{
    private readonly CommandHandler _apiHandlerGestion;
    private readonly ChannelReader<ServiceCommandeBaseApiAbstract> _channelApiReader;

    private readonly ChannelReader<ServiceCommandeBaseStrategyAbstract> _channelStrategyReader;

    private readonly ILogger _logger;

    public StrategyBackgroundService(ILogger logger,
        ChannelReader<ServiceCommandeBaseApiAbstract> channelApiReader,
        ChannelReader<ServiceCommandeBaseStrategyAbstract> channelStrategyReader,
        IMapper mapper,
        IEmailService emailService)
    {
        _logger = logger;
        _channelApiReader = channelApiReader;
        _channelStrategyReader = channelStrategyReader;
        _apiHandlerGestion = new CommandHandler(logger, mapper, emailService);
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var apiTask = ProcessApiChannel(stoppingToken);
        var strategyTask = ProcessStrategyChannel(stoppingToken);

        await Task.WhenAll(apiTask, strategyTask);
    }

    private async Task ProcessApiChannel(CancellationToken stoppingToken)
    {
        await foreach (var command in _channelApiReader.ReadAllAsync(stoppingToken))
            try
            {
                await _apiHandlerGestion.HandleApiCommand(command);
            }
            catch (System.Exception ex)
            {
                _logger.Error(ex, "Error on {command} execution", command);
                command.SetException(ex);
            }
    }

    private async Task ProcessStrategyChannel(CancellationToken stoppingToken)
    {
        await foreach (var command in _channelStrategyReader.ReadAllAsync(stoppingToken))
            try
            {
                await _apiHandlerGestion.HandleStrategyCommand(command);
            }
            catch (System.Exception ex)
            {
                _logger.Error(ex, "Error on {command} execution", command);
                command.SetException(ex);
            }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _apiHandlerGestion.Shutdown();
    }
}