using System.Threading.Channels;
using Robot.DataBase.Repositories;
using Robot.Server.Command.Strategy;
using Robot.Server.Command.Strategy.Request;
using Robot.Server.Dto.Request;
using Robot.Server.Dto.Response;
using RobotAppLibraryV2.Modeles;
using ILogger = Serilog.ILogger;

namespace Robot.Server.Services;

public class StrategyHandlerService : IStrategyHandlerService
{
    private readonly ChannelWriter<ServiceCommandeBaseStrategyAbstract> _channelStrategyWriter;
    private readonly ILogger _logger;
    private readonly IStrategyFileRepository _strategyFileRepository;

    public StrategyHandlerService(ILogger logger,
        ChannelWriter<ServiceCommandeBaseStrategyAbstract> channelStrategyWriter,
        IStrategyFileRepository strategyFileRepository)
    {
        _channelStrategyWriter = channelStrategyWriter;
        _strategyFileRepository = strategyFileRepository;

        _logger = logger.ForContext<StrategyHandlerService>();
    }

    public async Task InitStrategy(StrategyInitDto strategyInitDto)
    {
        var strategyFile = await _strategyFileRepository.GetByIdAsync(int.Parse(strategyInitDto.StrategyFileId));
        var initStrategyCommand = new InitStrategyCommand
        {
            StrategyFileDto = strategyFile,
            Symbol = strategyInitDto.Symbol,
            Timeframe = strategyInitDto.Timeframe,
            timeframe2 = strategyInitDto.Timeframe2
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


    public async Task<List<PositionDto>> GetStrategyPositionClosed(string id)
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


    public async Task<List<PositionDto>> GetOpenedPositions(string id)
    {
        var command = new GetOpenedPositionRequestCommand
        {
            Id = id
        };


        await _channelStrategyWriter.WriteAsync(command);

        var result = await command.ResponseSource.Task;

        return result.ListPositionsDto;
    }

    public async Task<BackTestDto> RunBackTest(string id, BackTestRequestDto backTestRequestDto)
    {
        var command = new RunStrategyBacktestCommand
        {
            Id = id,
            Balance = backTestRequestDto.Balance,
            MinSpread = backTestRequestDto.MinSpread,
            MaxSpread = backTestRequestDto.MaxSpread
        };

        await _channelStrategyWriter.WriteAsync(command);

        var result = await command.ResponseSource.Task;

        return result.BackTestDto;
    }

    public async Task<BackTestDto> GetBacktestResult(string id)
    {
        var command = new GetStrategyResultBacktestCommand
        {
            Id = id
        };

        await _channelStrategyWriter.WriteAsync(command);

        var result = await command.ResponseSource.Task;

        return result.BackTestDto;
    }
}