namespace StrategyApi.StrategyBackgroundService.Command.Api.Result;

public class GetTypeHandlerResultCommand : ServiceCommandResponse
{
    public string Handler { get; set; }
}