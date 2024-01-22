using System.Threading.Channels;
using Robot.Server.Command.Api;
using Robot.Server.Command.Api.Request;
using Robot.Server.Dto.Response;
using RobotAppLibraryV2.ApiHandler.Handlers.Enum;
using RobotAppLibraryV2.Modeles;
using ILogger = Serilog.ILogger;

namespace Robot.Server.Services;

public class ApiConnectService : IApiConnectService
{
    private readonly ChannelWriter<ServiceCommandeBaseApiAbstract> _channelApiWriter;
    private readonly ILogger _logger;

    public ApiConnectService(ILogger logger,
        ChannelWriter<ServiceCommandeBaseApiAbstract> channelWriter)
    {
        _channelApiWriter = channelWriter;
        _logger = logger.ForContext<ApiConnectService>();
    }


    public async Task Connect(ConnectDto connectDto)
    {
        var connecCommand = new ApiConnectCommand
        {
            ConnectDto = connectDto
        };


        await _channelApiWriter.WriteAsync(connecCommand);

        await connecCommand.ResponseSource.Task;
    }

    public async Task Disconnect()
    {
        var disconenctCommand = new DisconnectCommand();

        await _channelApiWriter.WriteAsync(disconenctCommand);

        await disconenctCommand.ResponseSource.Task;
    }

    public async Task<bool> IsConnected()
    {
        var isConnectedCommand = new IsConnectedCommand();

        await _channelApiWriter.WriteAsync(isConnectedCommand);

        var result = await isConnectedCommand.ResponseSource.Task;

        return result.IsConnected;
    }


    public async Task<string?> GetTypeHandler()
    {
        var getTypeHandlerCommand = new GetTypeHandlerCommand();

        await _channelApiWriter.WriteAsync(getTypeHandlerCommand);

        var result = await getTypeHandlerCommand.ResponseSource.Task;

        return result.Handler;
    }

    public Task<List<string>> GetListHandler()
    {
        return Task.FromResult(Enum.GetNames(typeof(ApiHandlerEnum)).ToList());
    }

    public async Task<List<SymbolInfo>> GetAllSymbol()
    {
        var getAllSymbolCommand = new GetAllSymbolCommand();

        await _channelApiWriter.WriteAsync(getAllSymbolCommand);

        var result = await getAllSymbolCommand.ResponseSource.Task;

        return result.SymbolInfos;
    }
}