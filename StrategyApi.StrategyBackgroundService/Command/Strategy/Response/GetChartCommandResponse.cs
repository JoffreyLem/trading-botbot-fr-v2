using StrategyApi.StrategyBackgroundService.Dto.Services;

namespace StrategyApi.StrategyBackgroundService.Command.Strategy.Response;

public class GetChartCommandResponse : ServiceCommandResponse
{
    public List<CandleDto> CandleDtos { get; set; }
}