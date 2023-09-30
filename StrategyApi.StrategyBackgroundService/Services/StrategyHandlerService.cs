using System.Threading.Channels;
using RobotAppLibraryV2.Modeles;
using StrategyApi.Dto.Dto;
using StrategyApi.Dto.Enum;
using StrategyApi.StrategyBackgroundService.Dto.Command.Result;
using StrategyApi.StrategyBackgroundService.Dto.Command.Strategy;
using ILogger = Serilog.ILogger;

namespace StrategyApi.StrategyBackgroundService.Services;

public class StrategyHandlerService : IStrategyHandlerService
{
    private readonly ILogger _logger;
    private ChannelWriter<(StrategyCommandBaseDto, TaskCompletionSource<CommandResultBase>)> _channelStrategyWriter;

    public StrategyHandlerService(ILogger logger,
        ChannelWriter<(StrategyCommandBaseDto, TaskCompletionSource<CommandResultBase>)> channelStrategyWriter)
    {
        _channelStrategyWriter = channelStrategyWriter;

        _logger = logger.ForContext<StrategyHandlerService>();
    }

    public async Task InitStrategy(StrategyTypeEnum strategyType, string symbol, Timeframe timeframe,
        Timeframe? timeframe2)
    {
        var tcs = new TaskCompletionSource<CommandResultBase>();

        InitStrategyCommandDto apiCommandDto = new InitStrategyCommandDto()
        {
            StrategyCommand = StrategyCommand.InitStrategy,
            StrategyType = strategyType,
            Symbol = symbol,
            Timeframe = timeframe,
            timeframe2 = timeframe2,
        };

        await _channelStrategyWriter.WriteAsync((apiCommandDto, tcs));

        await tcs.Task;
    }

    public async Task<IsInitializedDto> IsInitialized()
    {
        var tcs = new TaskCompletionSource<CommandResultBase>();

        StrategyCommandBaseDto apiCommandDto = new StrategyCommandBaseDto()
        {
            StrategyCommand = StrategyCommand.IsInitialized,
        };

        await _channelStrategyWriter.WriteAsync((apiCommandDto, tcs));

        var result = await tcs.Task as CommandExecutedTypedResult<IsInitializedDto>;

        return result.value;
    }

    public async Task<StrategyInfoDto> GetStrategyInfo()
    {
        var tcs = new TaskCompletionSource<CommandResultBase>();

        StrategyCommandBaseDto apiCommandDto = new StrategyCommandBaseDto()
        {
            StrategyCommand = StrategyCommand.GetStrategyInfo,
        };

        await _channelStrategyWriter.WriteAsync((apiCommandDto, tcs));

        var result = await tcs.Task as CommandExecutedTypedResult<StrategyInfoDto>;

        return result.value;
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
        var tcs = new TaskCompletionSource<CommandResultBase>();

        StrategyCommandBaseDto apiCommandDto = new StrategyCommandBaseDto()
        {
            StrategyCommand = StrategyCommand.CloseStrategy,
        };

        await _channelStrategyWriter.WriteAsync((apiCommandDto, tcs));

        await tcs.Task;
    }


    public async Task<ListPositionsDto> GetStrategyPosition()
    {
        var tcs = new TaskCompletionSource<CommandResultBase>();

        StrategyCommandBaseDto apiCommandDto = new StrategyCommandBaseDto()
        {
            StrategyCommand = StrategyCommand.GetStrategyPosition,
        };

        await _channelStrategyWriter.WriteAsync((apiCommandDto, tcs));

        var result = await tcs.Task as CommandExecutedTypedResult<ListPositionsDto>;

        return result.value;
    }

    public async Task<ResultDto> GetResult()
    {
        var tcs = new TaskCompletionSource<CommandResultBase>();

        StrategyCommandBaseDto apiCommandDto = new StrategyCommandBaseDto()
        {
            StrategyCommand = StrategyCommand.GetResults,
        };

        await _channelStrategyWriter.WriteAsync((apiCommandDto, tcs));

        var result = await tcs.Task as CommandExecutedTypedResult<ResultDto>;

        return result.value;
    }

    public async Task SetCanRun(bool value)
    {
        var tcs = new TaskCompletionSource<CommandResultBase>();

        StrategyBoolCommand apiCommandDto = new StrategyBoolCommand()
        {
            StrategyCommand = StrategyCommand.SetCanRun,
            Bool = value,
        };

        await _channelStrategyWriter.WriteAsync((apiCommandDto, tcs));

        await tcs.Task;
    }


    public async Task SetSecureControlPosition(bool value)
    {
        var tcs = new TaskCompletionSource<CommandResultBase>();

        StrategyBoolCommand apiCommandDto = new StrategyBoolCommand()
        {
            StrategyCommand = StrategyCommand.SetSecureControlPosition,
            Bool = value,
        };

        await _channelStrategyWriter.WriteAsync((apiCommandDto, tcs));

        await tcs.Task;
    }

    public async Task<ListPositionsDto> GetOpenedPositions()
    {
        var tcs = new TaskCompletionSource<CommandResultBase>();

        StrategyCommandBaseDto apiCommandDto = new StrategyCommandBaseDto()
        {
            StrategyCommand = StrategyCommand.GetOpenedPosition,
        };

        await _channelStrategyWriter.WriteAsync((apiCommandDto, tcs));

        var result = await tcs.Task as CommandExecutedTypedResult<ListPositionsDto>;

        return result.value;
    }
}