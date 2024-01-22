using Robot.Server.Dto.Response;

namespace Robot.Server.Command.Strategy.Response;

public class GetStrategyPositionClosedCommandResponse : ServiceCommandResponseBase
{
    public List<PositionDto> PositionDtos { get; set; }
}