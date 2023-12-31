using StrategyApi.StrategyBackgroundService.Dto;

namespace StrategyApi.StrategyBackgroundService.Services;

public interface IStrategyHandlerService
{
    Task InitStrategy(StrategyInitDto strategyInitDto);

    Task<List<string>> GetListTimeframes();
    Task<List<StrategyInfoDto>> GetAllStrategy();
    Task CloseStrategy(string id);
    Task<StrategyInfoDto> GetStrategyInfo(string id);
    Task<ListPositionsDto> GetStrategyPositionClosed(string id);
    Task<ResultDto> GetResult(string id);
    Task SetCanRun(string id, bool value);
    Task<ListPositionsDto> GetOpenedPositions(string id);
    Task<BackTestDto> RunBackTest(string id, double balance, decimal minspread, decimal maxspread);

    Task<BackTestDto> RunBacktestExternal(StrategyInitDto StrategyInitDto, double balance, decimal minspread,
        decimal maxspread);

    Task<BackTestDto> GetBacktestInfo(string id);
}