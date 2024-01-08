using RobotAppLibraryV2.ApiConnector.Connector.Websocket;
using RobotAppLibraryV2.ApiConnector.Executor;
using RobotAppLibraryV2.Modeles;

namespace RobotAppLibraryV2.Api.Xtb;

public class XtbCommandExecutor : WebsocketCommandExecutorBase
{
    public XtbCommandExecutor(WebsocketConnector tcpClient, WebsocketStreamingConnector tcpStreamingClient,
        CommandCreatorXtb commandCreator, XtbAdapter responseAdapter) : base(tcpClient, tcpStreamingClient,
        commandCreator, responseAdapter)
    {
    }

    public override async Task ExecuteLoginCommand(Credentials credentials)
    {
        await WebsocketConnector.ConnectAsync();
        var command = CommandCreator.CreateLoginCommand(credentials);
        var rsp = await WebsocketConnector.SendAndReceiveAsync(command);
        var rspAdapter = ResponseAdapter.AdaptLoginResponse(rsp);
        ((CommandCreatorXtb)CommandCreator).StreamingSessionId = rspAdapter.StreamingSessionId;
        await WebsocketStreamingConnector.ConnectAsync();
    }
}