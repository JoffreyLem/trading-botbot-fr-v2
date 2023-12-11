namespace StrategyApi.StrategyBackgroundService.Command.Strategy.Request;

public class SetCanRunCommand : ServiceCommandBaseStrategy<AcknowledgementResponse>
{
    public bool Bool { get; set; }
}