using RobotAppLibraryV2.ApiConnector.Connector.Tcp;
using RobotAppLibraryV2.ApiConnector.Executor;
using RobotAppLibraryV2.Modeles;

namespace RobotAppLibraryV2.Api.Xtb;

public class XtbCommandExecutor : TcpCommandExecutorBase
{
    public XtbCommandExecutor(TcpConnector tcpClient, StreamingClientXtb tcpStreamingClient,
        CommandCreatorXtb commandCreator, XtbAdapter responseAdapter) : base(tcpClient, tcpStreamingClient,
        commandCreator, responseAdapter)
    {
    }

    public override async Task ExecuteLoginCommand(Credentials credentials)
    {
        await TcpClient.ConnectAsync();
        var command = CommandCreator.CreateLoginCommand(credentials);
        var rsp = await TcpClient.SendAndReceiveAsync(command);
        var rspAdapter = ResponseAdapter.AdaptLoginResponse(rsp);
        ((CommandCreatorXtb)CommandCreator).StreamingSessionId = rspAdapter.StreamingSessionId;
        await TcpStreamingClient.ConnectAsync();
    }
}