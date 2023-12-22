using RobotAppLibraryV2.Modeles;
using StrategyApi.StrategyBackgroundService.Dto.Services;
using StrategyApi.StrategyBackgroundService.Dto.Services.Enum;

namespace StrategyApi.StrategyBackgroundService.Services;

public interface IStrategyHandlerService
{
    Task InitStrategy(StrategyTypeEnum strategyType, string symbol, Timeframe timeframe, Timeframe? timeframe2);
    Task<List<string>> GetListStrategy();
    Task<List<string>> GetListTimeframes();
    Task<List<StrategyInfoDto>> GetAllStrategy();
    Task CloseStrategy(string id);
    Task<StrategyInfoDto> GetStrategyInfo(string id);
    Task<ListPositionsDto> GetStrategyPositionClosed(string id);
    Task<ResultDto> GetResult(string id, bool isBackTest = false);
    Task SetCanRun(string id, bool value);
    Task<ListPositionsDto> GetOpenedPositions(string id);
    Task<BackTestDto> RunBackTest(string id, double balance, decimal minspread, decimal maxspread);
    Task<BackTestDto> GetBacktestInfo(string id);
}