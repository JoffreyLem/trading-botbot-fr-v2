using StrategyApi.StrategyBackgroundService.Dto;

namespace StrategyApi.StrategyBackgroundService.Command.Strategy.Response;

public class GetAllStrategyCommandResponse : ServiceCommandResponseBase
{
    public List<StrategyInfoDto> ListStrategyInfoDto { get; set; }
}