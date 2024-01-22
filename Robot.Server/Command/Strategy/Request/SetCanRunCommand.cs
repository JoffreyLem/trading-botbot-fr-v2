namespace Robot.Server.Command.Strategy.Request;

public class SetCanRunCommand : ServiceCommandBaseStrategy<AcknowledgementResponse>
{
    public bool Bool { get; set; }
}