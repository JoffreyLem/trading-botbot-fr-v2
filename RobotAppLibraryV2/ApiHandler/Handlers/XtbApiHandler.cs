using RobotAppLibraryV2.ApiConnector.Interfaces;
using Serilog;

namespace RobotAppLibraryV2.ApiHandler.Handlers;

public class XtbApiHandler : ApiHandlerBase
{
    public XtbApiHandler(ICommandExecutor commandExecutor, ILogger logger) : base(commandExecutor, logger)
    {
    }

    protected override TimeSpan PingInterval => TimeSpan.FromMinutes(9);
}