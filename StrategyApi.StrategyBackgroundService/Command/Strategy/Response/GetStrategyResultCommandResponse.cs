using StrategyApi.StrategyBackgroundService.Dto.Services;

namespace StrategyApi.StrategyBackgroundService.Command.Strategy.Response;

public class GetStrategyResultCommandResponse : ServiceCommandResponseBase
{
    public ResultDto ResultDto { get; set; }
}