using RobotAppLibraryV2.Modeles;
using StrategyApi.StrategyBackgroundService.Dto.Services;
using StrategyApi.StrategyBackgroundService.Dto.Services.Enum;

namespace StrategyApi.StrategyBackgroundService.Services;

public interface IStrategyHandlerService
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