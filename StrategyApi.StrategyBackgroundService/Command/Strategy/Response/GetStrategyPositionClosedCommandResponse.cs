using StrategyApi.StrategyBackgroundService.Dto.Services;

namespace StrategyApi.StrategyBackgroundService.Command.Strategy.Response;

public class GetStrategyPositionClosedCommandResponse : ServiceCommandResponseBase
{
    public ListPositionsDto PositionDtos { get; set; }
}