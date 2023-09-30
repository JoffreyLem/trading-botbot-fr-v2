namespace StrategyApi.StrategyBackgroundService.Dto.Command.Api;

public enum ApiCommand
{
    InitHandler = 0,
    GetTypeHandler = 1,
    GetAllSymbols = 2,
    IsConnected = 3,
    Connect = 4,
    Disconnect = 5
}