namespace Robot.Server.Command.Api.Result;

public class IsConnectedResultCommand : ServiceCommandResponseBase
{
    public bool IsConnected { get; set; }
}