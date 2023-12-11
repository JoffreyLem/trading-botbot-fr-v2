using System.Threading.Channels;
using RobotAppLibraryV2.Modeles;
using Serilog;
using StrategyApi.StrategyBackgroundService.Command.Strategy;
using StrategyApi.StrategyBackgroundService.Command.Strategy.Request;
using StrategyApi.StrategyBackgroundService.Dto.Services;
using StrategyApi.StrategyBackgroundService.Dto.Services.Enum;

namespace StrategyApi.StrategyBackgroundService.Services;

public class StrategyHandlerService : IStrategyHandlerService
{
    private readonly ChannelWriter<ServiceCommandeBaseStrategyAbstract> _channelStrategyWriter;

    private readonly ILogger _logger;

    public StrategyHandlerService(ILogger logger,
        ChannelWriter<ServiceCommandeBaseStrategyAbstract> channelStrategyWriter)
    {
        _channelStrategyWriter = channelStrategyWriter;

        _logger = logger.ForContext<StrategyHandlerService>();
    }

    public async Task InitStrategy(StrategyTypeEnum strategyType, string symbol, Timeframe timeframe,
        Timeframe? timeframe2)
    {
        var initStrategyCommand = new InitStrategyCommand
        {
            StrategyType = strategyType,
            Symbol = symbol,
            Timeframe = timeframe,
            timeframe2 = timeframe2
        };


        await _channelStrategyWriter.WriteAsync(initStrategyCommand);

        await initStrategyCommand.ResponseSource.Task;
    }

    public async Task<IsInitializedDto> IsInitialized()
    {
        var isInitializedCommand = new IsInitializerCommand();

        await _channelStrategyWriter.WriteAsync(isInitializedCommand);

        var result = await isInitializedCommand.ResponseSource.Task;

        return new IsInitializedDto
        {
            Initialized = result.IsInitialized
        };
    }

    public async Task<StrategyInfoDto> GetStrategyInfo()
    {
        var getStrategyInfoCommand = new GetStrategyInfoCommand();

        await _channelStrategyWriter.WriteAsync(getStrategyInfoCommand);

        var result = await getStrategyInfoCommand.ResponseSource.Task;

        return result.StrategyInfoDto;
    }

    public Task<List<string>> GetListStrategy()
    {
        return Task.FromResult(Enum.GetNames(typeof(StrategyTypeEnum)).ToList());
    }

    public Task<List<string>> GetListTimeframes()
    {
        return Task.FromResult(Enum.GetNames(typeof(Timeframe)).ToList());
    }

    public async Task CloseStrategy()
    {
        var closeStrategyCommand = new CloseStrategyCommand();

        await _channelStrategyWriter.WriteAsync(closeStrategyCommand);

        await closeStrategyCommand.ResponseSource.Task;
    }


    public async Task<ListPositionsDto> GetStrategyPositionClosed()
    {
        var getStrategyPositionClosed = new GetStrategyPositionClosedCommand();

        await _channelStrategyWriter.WriteAsync(getStrategyPositionClosed);

        var result = await getStrategyPositionClosed.ResponseSource.Task;

        return result.PositionDtos;
    }

    public async Task<ResultDto> GetResult()
    {
        var resultCommand = new GetStrategyResultRequestCommand();

        await _channelStrategyWriter.WriteAsync(resultCommand);

        var result = await resultCommand.ResponseSource.Task;

        return result.ResultDto;
    }

    public async Task SetCanRun(bool value)
    {
        var setCanRunCommand = new SetCanRunCommand
        {
            Bool = value
        };


        await _channelStrategyWriter.WriteAsync(setCanRunCommand);

        await setCanRunCommand.ResponseSource.Task;
    }


    public async Task<ListPositionsDto> GetOpenedPositions()
    {
        var command = new GetOpenedPositionRequestCommand();


        await _channelStrategyWriter.WriteAsync(command);

        var result = await command.ResponseSource.Task;

        return result.ListPositionsDto;
    }

    public async Task<List<CandleDto>> GetChart()
    {
        var getChartCommand = new GetChartCommandRequest();

        await _channelStrategyWriter.WriteAsync(getChartCommand);

        var result = await getChartCommand.ResponseSource.Task;

        return result.CandleDtos;
    }
}