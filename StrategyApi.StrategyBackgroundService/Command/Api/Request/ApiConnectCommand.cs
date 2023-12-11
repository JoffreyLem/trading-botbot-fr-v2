using RobotAppLibraryV2.Modeles;

namespace StrategyApi.StrategyBackgroundService.Command.Api.Request;

public class ApiConnectCommand : ServiceCommandBaseApi<AcknowledgementResponse>
{
    public Credentials Credentials { get; set; }
}