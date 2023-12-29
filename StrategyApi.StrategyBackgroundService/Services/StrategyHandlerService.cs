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

    public async Task<StrategyInfoDto> GetStrategyInfo(string id)
    {
        var getStrategyInfoCommand = new GetStrategyInfoCommand
        {
            Id = id
        };

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

    public async Task<List<StrategyInfoDto>> GetAllStrategy()
    {
        var allStrategyCommand = new GetAllStrategyCommandRequest();

        await _channelStrategyWriter.WriteAsync(allStrategyCommand);

        return (await allStrategyCommand.ResponseSource.Task).ListStrategyInfoDto;
    }

    public async Task CloseStrategy(string id)
    {
        var closeStrategyCommand = new CloseStrategyCommand
        {
            Id = id
        };

        await _channelStrategyWriter.WriteAsync(closeStrategyCommand);

        await closeStrategyCommand.ResponseSource.Task;
    }


    public async Task<ListPositionsDto> GetStrategyPositionClosed(string id)
    {
        var getStrategyPositionClosed = new GetStrategyPositionClosedCommand
        {
            Id = id
        };

        await _channelStrategyWriter.WriteAsync(getStrategyPositionClosed);

        var result = await getStrategyPositionClosed.ResponseSource.Task;

        return result.PositionDtos;
    }

    public async Task<ResultDto> GetResult(string id)
    {
        var resultCommand = new GetStrategyResultRequestCommand
        {
            Id = id
        };

        await _channelStrategyWriter.WriteAsync(resultCommand);

        var result = await resultCommand.ResponseSource.Task;

        return result.ResultDto;
    }

    public async Task SetCanRun(string id, bool value)
    {
        var setCanRunCommand = new SetCanRunCommand
        {
            Bool = value,
            Id = id
        };


        await _channelStrategyWriter.WriteAsync(setCanRunCommand);

        await setCanRunCommand.ResponseSource.Task;
    }


    public async Task<ListPositionsDto> GetOpenedPositions(string id)
    {
        var command = new GetOpenedPositionRequestCommand
        {
            Id = id
        };


        await _channelStrategyWriter.WriteAsync(command);

        var result = await command.ResponseSource.Task;

        return result.ListPositionsDto;
    }

    public async Task<BackTestDto> RunBackTest(string id, double balance, decimal minspread, decimal maxspread)
    {
        var command = new RunStrategyBacktestCommand
        {
            Id = id,
            Balance = balance,
            MinSpread = minspread,
            MaxSpread = maxspread
        };

        await _channelStrategyWriter.WriteAsync(command);

        var result = await command.ResponseSource.Task;

        return result.BackTestDto;
    }

    public async Task<BackTestDto> RunBacktestExternal(StrategyInitDto strategyInitDto, double balance,
        decimal minspread, decimal maxspread)
    {
        var command = new RunStrategyBacktestExternalCommand
        {
            StrategyType = strategyInitDto.StrategyType,
            Symbol = strategyInitDto.Symbol,
            Timeframe = strategyInitDto.Timeframe,
            Timeframe2 = strategyInitDto.Timeframe2,
            Balance = balance,
            MinSpread = minspread,
            MaxSpread = maxspread
        };

        await _channelStrategyWriter.WriteAsync(command);

        var result = await command.ResponseSource.Task;

        return result.BackTestDto;
    }

    public async Task<BackTestDto> GetBacktestInfo(string id)
    {
        var command = new GetBacktestInfoCommand
        {
            Id = id
        };

        await _channelStrategyWriter.WriteAsync(command);

        var result = await command.ResponseSource.Task;

        return result.BackTestDto;
    }
}