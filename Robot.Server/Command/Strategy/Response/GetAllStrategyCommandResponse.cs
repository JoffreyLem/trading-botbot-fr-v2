using Robot.Server.Dto.Response;

namespace Robot.Server.Command.Strategy.Response;

public class GetAllStrategyCommandResponse : ServiceCommandResponseBase
{
    public List<StrategyInfoDto> ListStrategyInfoDto { get; set; }
}