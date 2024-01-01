using StrategyApi.StrategyBackgroundService.Command.Strategy.Response;

namespace StrategyApi.StrategyBackgroundService.Command.Strategy.Request;

public class RunStrategyBacktestCommand : ServiceCommandBaseStrategy<BacktestCommandResponse>
{
    public double Balance { get; set; }

    public decimal MinSpread { get; set; }

    public decimal MaxSpread { get; set; }
}