using Robot.Server.Dto.Response;
using RobotAppLibraryV2.Modeles;

namespace Robot.Server.Services;

public interface IApiConnectService
{
    Task Connect(ConnectDto connectDto);

    Task Disconnect();

    Task<bool> IsConnected();

    Task<string?> GetTypeHandler();

    Task<List<string>> GetListHandler();

    Task<List<SymbolInfo>> GetAllSymbol();
}