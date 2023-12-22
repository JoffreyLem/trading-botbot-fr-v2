using StrategyApi.StrategyBackgroundService.Dto.Services;

namespace StrategyApi.StrategyBackgroundService.Command.Strategy.Response;

public class BacktestCommandResponse : ServiceCommandResponse
{
    public BackTestDto BackTestDto { get; set; }
}