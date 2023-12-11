using RobotAppLibraryV2.ApiHandler.Handlers.Enum;

namespace StrategyApi.StrategyBackgroundService.Command.Api.Request;

public class InitHandlerCommand : ServiceCommandBaseApi<AcknowledgementResponse>
{
    public ApiHandlerEnum ApiHandlerEnum { get; set; }
}