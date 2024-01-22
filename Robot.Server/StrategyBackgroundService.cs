using System.Threading.Channels;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Robot.Mail;
using Robot.Server.Command.Api;
using Robot.Server.Command.Strategy;
using Robot.Server.Command.Strategy.Request;
using Robot.Server.Hubs;
using ILogger = Serilog.ILogger;

namespace Robot.Server;

public class StrategyBackgroundService : BackgroundService
{
    private readonly ChannelReader<ServiceCommandeBaseApiAbstract> _channelApiReader;

    private readonly ChannelReader<ServiceCommandeBaseStrategyAbstract> _channelStrategyReader;
    private readonly CommandHandler _commandHandler;

    private readonly ILogger _logger;

    public StrategyBackgroundService(ILogger logger,
        ChannelReader<ServiceCommandeBaseApiAbstract> channelApiReader,
        ChannelReader<ServiceCommandeBaseStrategyAbstract> channelStrategyReader,
        IMapper mapper,
        IEmailService emailService,
        IHubContext<HubInfoClient, IHubInfoClient> hubContext)
    {
        _logger = logger;
        _channelApiReader = channelApiReader;
        _channelStrategyReader = channelStrategyReader;
        _commandHandler = new CommandHandler(logger, mapper, emailService, hubContext);
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
                _logger.Information("Strategy command received {Command}", command);
                await _commandHandler.HandleApiCommand(command);
            }
            catch (System.Exception ex)
            {
                _logger.Error(ex, "Error on {Command} execution", command);
                command.SetException(ex);
            }

        Console.WriteLine("ok");
    }

    private async Task ProcessStrategyChannel(CancellationToken stoppingToken)
    {
        await foreach (var command in _channelStrategyReader.ReadAllAsync(stoppingToken))
            try
            {
                _logger.Information("Api command received {Command}", command);
                if (command is RunStrategyBacktestCommand)
                    _ = _commandHandler.HandleStrategyCommand(command);
                else
                    await _commandHandler.HandleStrategyCommand(command);
            }
            catch (System.Exception ex)
            {
                _logger.Error(ex, "Error on {Command} execution", command);
                command.SetException(ex);
            }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _commandHandler.Shutdown();
    }
}