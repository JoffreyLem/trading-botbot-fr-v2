using StrategyApi.StrategyBackgroundService.Dto.Services.Enum;

namespace StrategyApi.StrategyBackgroundService.Command.Api.Result;

public class IsConnectedResultCommand : ServiceCommandResponse
{
    public ConnexionStateEnum ConnexionStateEnum { get; set; }
}