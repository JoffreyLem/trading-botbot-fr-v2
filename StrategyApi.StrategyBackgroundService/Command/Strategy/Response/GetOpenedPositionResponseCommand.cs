using StrategyApi.StrategyBackgroundService.Dto.Services;

namespace StrategyApi.StrategyBackgroundService.Command.Strategy.Response;

public class GetOpenedPositionResponseCommand : ServiceCommandResponseBase
{
    public ListPositionsDto ListPositionsDto { get; set; }
}