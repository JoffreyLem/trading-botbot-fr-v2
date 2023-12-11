using StrategyApi.StrategyBackgroundService.Dto.Services;

namespace StrategyApi.StrategyBackgroundService.Command.Strategy.Response;

public class GetStrategyResultCommandResponse : ServiceCommandResponse
{
    public ResultDto ResultDto { get; set; }
}