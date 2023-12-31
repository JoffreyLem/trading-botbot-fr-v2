using StrategyApi.StrategyBackgroundService.Dto.Enum;

namespace StrategyApi.StrategyBackgroundService.Command.Api.Result;

public class IsConnectedResultCommand : ServiceCommandResponseBase
{
    public ConnexionStateEnum ConnexionStateEnum { get; set; }
}