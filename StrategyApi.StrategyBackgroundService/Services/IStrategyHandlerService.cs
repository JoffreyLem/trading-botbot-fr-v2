using RobotAppLibraryV2.Modeles;
using StrategyApi.Dto.Dto;
using StrategyApi.Dto.Enum;

namespace StrategyApi.StrategyBackgroundService.Services;

internal interface IStrategyHandlerService
{
    Task InitStrategy(StrategyTypeEnum strategyType, string symbol, Timeframe timeframe, Timeframe? timeframe2);
    
    Task<IsInitializedDto> IsInitialized();

    Task<List<string>> GetListStrategy();
    Task<List<string>> GetListTimeframes();
    Task CloseStrategy();

    Task<StrategyInfoDto> GetStrategyInfo();

    Task<ListPositionsDto> GetStrategyPosition();

    Task<ResultDto> GetResult();

    Task SetCanRun(bool value);

    Task SetSecureControlPosition(bool value);

    Task<ListPositionsDto> GetOpenedPositions();
}