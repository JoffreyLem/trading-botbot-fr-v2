using Robot.Server.Dto.Response;

namespace Robot.Server.Command.Strategy.Response;

public class GetStrategyInfoCommandResponse : ServiceCommandResponseBase
{
    public StrategyInfoDto StrategyInfoDto { get; set; }
}