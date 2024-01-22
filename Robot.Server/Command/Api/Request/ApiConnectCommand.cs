using Robot.Server.Dto.Response;

namespace Robot.Server.Command.Api.Request;

public class ApiConnectCommand : ServiceCommandBaseApi<AcknowledgementResponse>
{
    public ConnectDto ConnectDto { get; set; }
}