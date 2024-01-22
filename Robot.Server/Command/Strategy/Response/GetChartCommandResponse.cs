using Robot.Server.Dto.Response;

namespace Robot.Server.Command.Strategy.Response;

public class GetChartCommandResponse : ServiceCommandResponseBase
{
    public List<CandleDto> CandleDtos { get; set; }
}