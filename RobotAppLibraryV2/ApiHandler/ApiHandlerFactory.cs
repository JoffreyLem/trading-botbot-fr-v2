using RobotAppLibraryV2.Api.Xtb;
using RobotAppLibraryV2.ApiConnector.Modeles;
using RobotAppLibraryV2.ApiConnector.Tcp;
using RobotAppLibraryV2.ApiHandler.Handlers;
using RobotAppLibraryV2.ApiHandler.Handlers.Enum;
using RobotAppLibraryV2.ApiHandler.Interfaces;
using Serilog;

namespace RobotAppLibraryV2.ApiHandler;

public class ApiHandlerFactory
{
    public static IApiHandler GetApiHandler(ApiHandlerEnum api, Server server, ILogger logger)
    {
        return api switch
        {
            ApiHandlerEnum.Xtb => GetXtbApiHandler(server, logger),
            _ => throw new ArgumentException($"{api.ToString()} not handled")
        };
    }

    private static IApiHandler GetXtbApiHandler(Server server, ILogger logger)
    {
        var tcpConnector = new TcpConnector(server, logger);
        var adapter = new XtbAdapter();
        var streamingCLient = new StreamingClientXtb(server, logger, adapter);
        var commandCreator = new CommandCreatorXtb();
        var icommandExecutor = new XtbCommandExecutor(tcpConnector, streamingCLient, commandCreator, adapter);
        return new XtbApiHandler(icommandExecutor, logger);
    }
}