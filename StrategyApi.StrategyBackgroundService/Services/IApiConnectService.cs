using RobotAppLibraryV2.ApiHandler.Handlers.Enum;
using RobotAppLibraryV2.Modeles;
using StrategyApi.StrategyBackgroundService.Dto.Enum;

namespace StrategyApi.StrategyBackgroundService.Services;

public interface IApiConnectService
{
    Task Connect(string user, string pwd);

    Task Disconnect();

    Task<ConnexionStateEnum> IsConnected();

    Task InitHandler(ApiHandlerEnum @enum);

    Task<string?> GetTypeHandler();

    Task<List<string>> GetListHandler();

    Task<List<SymbolInfo>> GetAllSymbol();
}