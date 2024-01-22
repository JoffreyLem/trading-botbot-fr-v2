using Robot.Server.Dto.Response;

namespace Robot.Server.Command.Strategy.Response;

public class GetStrategyResultCommandResponse : ServiceCommandResponseBase
{
    public ResultDto ResultDto { get; set; }
}