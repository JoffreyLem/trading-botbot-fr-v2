namespace StrategyApi.StrategyBackgroundService.Dto.Services;

public class CandleDto
{
    public double BidVolume { get; set; }
    public double AskVolume { get; set; }
    public DateTime Date { get; set; }

    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public decimal Volume { get; set; }
}