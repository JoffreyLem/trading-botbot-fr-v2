using System.Threading.Channels;
using StrategyApi.StrategyBackgroundService.Dto.Command.Api;
using StrategyApi.StrategyBackgroundService.Dto.Command.Result;
using StrategyApi.StrategyBackgroundService.Dto.Services.Enum;
using ILogger = Serilog.ILogger;

namespace StrategyApi.StrategyBackgroundService.Services;

public class ApiConnectService : IApiConnectService
{
    private readonly ILogger _logger;
    private ChannelWriter<(ApiCommandBaseDto, TaskCompletionSource<CommandResultBase>)> _channelApiWriter;

    public ApiConnectService(ILogger logger,
        ChannelWriter<(ApiCommandBaseDto, TaskCompletionSource<CommandResultBase>)> channelWriter)
    {
        _channelApiWriter = channelWriter;
        _logger = logger.ForContext<ApiConnectService>();
    }


    public async Task Connect(string user, string pwd)
    {
        var tcs = new TaskCompletionSource<CommandResultBase>();

        ApiConnectCommandDto apiCommandDto = new ApiConnectCommandDto()
        {
            ApiCommandEnum = ApiCommand.Connect,
            User = user,
            Password = pwd,
        };

        await _channelApiWriter.WriteAsync((apiCommandDto, tcs));

        await tcs.Task;
    }

    public async Task Disconnect()
    {
        var tcs = new TaskCompletionSource<CommandResultBase>();

        ApiCommandBaseDto apiCommandDto = new ApiCommandBaseDto()
        {
            ApiCommandEnum = ApiCommand.Disconnect,
        };

        await _channelApiWriter.WriteAsync((apiCommandDto, tcs));

        await tcs.Task;
    }

    public async Task<ConnexionStateEnum> IsConnected()
    {
        var tcs = new TaskCompletionSource<CommandResultBase>();

        ApiCommandBaseDto apiCommandDto = new ApiCommandBaseDto()
        {
            ApiCommandEnum = ApiCommand.IsConnected,
        };

        await _channelApiWriter.WriteAsync((apiCommandDto, tcs));

        var result = await tcs.Task as CommandExecutedTypedResult<ConnexionStateEnum>;

        return result.value;
    }


    public async Task InitHandler(ApiHandlerEnum @enum)
    {
        var tcs = new TaskCompletionSource<CommandResultBase>();

        InitHandlerCommandDto apiCommandDto = new InitHandlerCommandDto()
        {
            ApiCommandEnum = ApiCommand.InitHandler,
            ApiHandlerEnum = @enum,
        };

        await _channelApiWriter.WriteAsync((apiCommandDto, tcs));

        await tcs.Task;
    }

    public async Task<string?> GetTypeHandler()
    {
        var tcs = new TaskCompletionSource<CommandResultBase>();

        ApiCommandBaseDto apiCommandDto = new ApiCommandBaseDto()
        {
            ApiCommandEnum = ApiCommand.GetTypeHandler,
        };

        await _channelApiWriter.WriteAsync((apiCommandDto, tcs));

        var result = await tcs.Task as CommandExecutedTypedResult<string?>;

        return result.value;
    }

    public Task<List<string>> GetListHandler()
    {
        return Task.FromResult(Enum.GetNames(typeof(ApiHandlerEnum)).ToList());
    }

    public async Task<List<string>?> GetAllSymbol()
    {
        var tcs = new TaskCompletionSource<CommandResultBase>();

        ApiCommandBaseDto apiCommandDto = new ApiCommandBaseDto()
        {
            ApiCommandEnum = ApiCommand.GetAllSymbols,
        };

        await _channelApiWriter.WriteAsync((apiCommandDto, tcs));

        var result = await tcs.Task as CommandExecutedTypedResult<List<string>>;

        return result.value;
    }
}