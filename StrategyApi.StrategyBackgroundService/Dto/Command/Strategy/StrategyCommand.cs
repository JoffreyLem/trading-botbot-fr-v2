namespace StrategyApi.StrategyBackgroundService.Dto.Command.Strategy;

public enum StrategyCommand
{
    InitStrategy = 0,
    IsInitialized = 1,
    GetStrategyInfo = 2,
    CloseStrategy = 3,
    GetStrategyPosition = 4,
    GetResults = 5,
    GetOpenedPosition = 6,
    SetCanRun = 7,
    SetSecureControlPosition = 8
}