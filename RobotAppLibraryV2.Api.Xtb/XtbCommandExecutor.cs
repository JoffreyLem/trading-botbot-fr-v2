using RobotAppLibraryV2.ApiConnector.Tcp;
using RobotAppLibraryV2.Modeles;

namespace RobotAppLibraryV2.Api.Xtb;

public class
    XtbCommandExecutor : TcpCommandExecutorBase<TcpConnector, StreamingClientXtb, CommandCreatorXtb, XtbAdapter>
{
    public XtbCommandExecutor(TcpConnector tcpClient, StreamingClientXtb tcpStreamingClient,
        CommandCreatorXtb commandCreator, XtbAdapter responseAdapter) : base(tcpClient, tcpStreamingClient,
        commandCreator, responseAdapter)
    {
    }

    public override async Task ExecuteLoginCommand(Credentials credentials)
    {
        var command = commandCreator.CreateLoginCommand(credentials);
        var rsp = await tcpClient.SendAndReceiveAsync(command);
        var rspAdapter = _responseAdapter.AdaptLoginResponse(rsp);
        commandCreator.StreamingSessionId = rspAdapter.StreamingSessionId;
    }
}