namespace StrategyApi.StrategyBackgroundService.Command.Api.Result;

public class GetTypeHandlerResultCommand : ServiceCommandResponseBase
{
    public string Handler { get; set; }
}