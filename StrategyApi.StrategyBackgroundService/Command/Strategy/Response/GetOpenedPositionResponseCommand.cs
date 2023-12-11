using StrategyApi.StrategyBackgroundService.Dto.Services;

namespace StrategyApi.StrategyBackgroundService.Command.Strategy.Response;

public class GetOpenedPositionResponseCommand : ServiceCommandResponse
{
    public ListPositionsDto ListPositionsDto { get; set; }
}