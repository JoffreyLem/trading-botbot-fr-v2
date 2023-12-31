using StrategyApi.StrategyBackgroundService.Dto;

namespace StrategyApi.StrategyBackgroundService.Command.Strategy.Response;

public class BacktestCommandResponse : ServiceCommandResponseBase
{
    public BackTestDto BackTestDto { get; set; }
}