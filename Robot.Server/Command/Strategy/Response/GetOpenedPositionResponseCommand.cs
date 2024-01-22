using Robot.Server.Dto.Response;

namespace Robot.Server.Command.Strategy.Response;

public class GetOpenedPositionResponseCommand : ServiceCommandResponseBase
{
    public List<PositionDto> ListPositionsDto { get; set; }
}