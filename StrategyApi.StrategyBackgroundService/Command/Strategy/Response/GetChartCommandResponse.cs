using StrategyApi.StrategyBackgroundService.Dto;

namespace StrategyApi.StrategyBackgroundService.Command.Strategy.Response;

public class GetChartCommandResponse : ServiceCommandResponseBase
{
    public List<CandleDto> CandleDtos { get; set; }
}