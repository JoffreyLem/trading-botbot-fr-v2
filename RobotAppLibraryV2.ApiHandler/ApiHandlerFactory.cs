using RobotAppLibraryV2.Api.Xtb;
using RobotAppLibraryV2.ApiConnector.Connector.Websocket;
using RobotAppLibraryV2.ApiHandler.Handlers;
using RobotAppLibraryV2.ApiHandler.Handlers.Enum;
using Serilog;

namespace RobotAppLibraryV2.ApiHandler;

public static class ApiHandlerFactory
{
    public static IApiHandler GetApiHandler(ApiHandlerEnum api, ILogger logger)
    {
        return api switch
        {
            ApiHandlerEnum.Xtb => GetXtbApiHandler(logger),
            _ => throw new ArgumentException($"{api.ToString()} not handled")
        };
    }

    private static IApiHandler GetXtbApiHandler(ILogger logger)
    {
        var tcpConnector = new WebsocketConnector(XtbServer.DEMO_WSS.Address, logger);
        var adapter = new XtbAdapter();
        var streamingCLient = new StreamingClientXtb(XtbServer.DEMO_WSS_STREAMING.Address, logger, adapter);
        var commandCreator = new CommandCreatorXtb();
        var icommandExecutor = new XtbCommandExecutor(tcpConnector, streamingCLient, commandCreator, adapter);
        return new XtbApiHandler(icommandExecutor, logger);
    }
}