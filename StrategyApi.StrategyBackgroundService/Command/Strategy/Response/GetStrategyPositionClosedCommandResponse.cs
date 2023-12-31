using StrategyApi.StrategyBackgroundService.Dto;

namespace StrategyApi.StrategyBackgroundService.Command.Strategy.Response;

public class GetStrategyPositionClosedCommandResponse : ServiceCommandResponseBase
{
    public ListPositionsDto PositionDtos { get; set; }
}