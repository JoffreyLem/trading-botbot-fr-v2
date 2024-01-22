using Robot.Server.Dto.Response;

namespace Robot.Server.Command.Strategy.Response;

public class GetStrategyResultCommandBacktestResponse : ServiceCommandResponseBase
{
    public BackTestDto? BackTestDto { get; set; }
}