using StrategyApi.Dto.Enum;

namespace StrategyApi.StrategyBackgroundService.Services;

public interface IApiConnectService
{
    Task Connect(string user, string pwd);

    Task Disconnect();

    Task<ConnexionStateEnum> IsConnected();

    Task InitHandler(ApiHandlerEnum @enum);

    Task<string?> GetTypeHandler();

    Task<List<string>> GetListHandler();

    Task<List<string>?> GetAllSymbol();
}