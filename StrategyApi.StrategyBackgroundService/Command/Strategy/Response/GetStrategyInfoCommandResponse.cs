using StrategyApi.StrategyBackgroundService.Dto.Services;

namespace StrategyApi.StrategyBackgroundService.Command.Strategy.Response;

public class GetStrategyInfoCommandResponse : ServiceCommandResponse
{
    public StrategyInfoDto StrategyInfoDto { get; set; }
}