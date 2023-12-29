using StrategyApi.StrategyBackgroundService.Dto.Services;

namespace StrategyApi.StrategyBackgroundService.Command.Strategy.Response;

public class BacktestCommandResponse : ServiceCommandResponseBase
{
    public BackTestDto BackTestDto { get; set; }
}