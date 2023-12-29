using StrategyApi.StrategyBackgroundService.Dto.Services.Enum;

namespace StrategyApi.StrategyBackgroundService.Command.Api.Result;

public class IsConnectedResultCommand : ServiceCommandResponseBase
{
    public ConnexionStateEnum ConnexionStateEnum { get; set; }
}