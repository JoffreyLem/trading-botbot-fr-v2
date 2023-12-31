using StrategyApi.StrategyBackgroundService.Dto;

namespace StrategyApi.StrategyBackgroundService.Command.Strategy.Response;

public class GetStrategyInfoCommandResponse : ServiceCommandResponseBase
{
    public StrategyInfoDto StrategyInfoDto { get; set; }
}