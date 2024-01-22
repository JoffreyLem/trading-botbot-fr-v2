using Robot.Server.Dto.Request;
using Robot.Server.Dto.Response;

namespace Robot.Server.Services;

public interface IStrategyHandlerService
{
    Task InitStrategy(StrategyInitDto strategyInitDto);

    Task<List<string>> GetListTimeframes();
    Task<List<StrategyInfoDto>> GetAllStrategy();
    Task CloseStrategy(string id);
    Task<StrategyInfoDto> GetStrategyInfo(string id);
    Task<List<PositionDto>> GetStrategyPositionClosed(string id);
    Task<ResultDto> GetResult(string id);
    Task SetCanRun(string id, bool value);
    Task<List<PositionDto>> GetOpenedPositions(string id);
    Task<BackTestDto> RunBackTest(string id, BackTestRequestDto backTestRequestDto);

    Task<BackTestDto> GetBacktestResult(string id);
}