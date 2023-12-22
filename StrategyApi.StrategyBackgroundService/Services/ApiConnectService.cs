using System.Threading.Channels;
using RobotAppLibraryV2.ApiHandler.Handlers.Enum;
using RobotAppLibraryV2.Modeles;
using Serilog;
using StrategyApi.StrategyBackgroundService.Command.Api;
using StrategyApi.StrategyBackgroundService.Command.Api.Request;
using StrategyApi.StrategyBackgroundService.Dto.Services.Enum;

namespace StrategyApi.StrategyBackgroundService.Services;

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


    public async Task Connect(string user, string pwd)
    {
       
            var connecCommand = new ApiConnectCommand
            {
                Credentials = new Credentials
                {
                    User = user,
                    Password = pwd
                }
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

    public async Task<ConnexionStateEnum> IsConnected()
    {
        var isConnectedCommand = new IsConnectedCommand();

        await _channelApiWriter.WriteAsync(isConnectedCommand);

        var result = await isConnectedCommand.ResponseSource.Task;

        return result.ConnexionStateEnum;
    }


    public async Task InitHandler(ApiHandlerEnum @enum)
    {
        var initHandlerCommand = new InitHandlerCommand
        {
            ApiHandlerEnum = @enum
        };

        await _channelApiWriter.WriteAsync(initHandlerCommand);

        await initHandlerCommand.ResponseSource.Task;
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