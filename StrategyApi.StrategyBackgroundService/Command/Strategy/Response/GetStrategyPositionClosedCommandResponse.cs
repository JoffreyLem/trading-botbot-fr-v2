using StrategyApi.StrategyBackgroundService.Dto.Services;

namespace StrategyApi.StrategyBackgroundService.Command.Strategy.Response;

public class GetStrategyPositionClosedCommandResponse : ServiceCommandResponse
{
    public ListPositionsDto PositionDtos { get; set; }
}