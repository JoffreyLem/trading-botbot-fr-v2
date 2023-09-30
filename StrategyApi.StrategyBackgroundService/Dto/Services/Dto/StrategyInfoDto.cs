namespace StrategyApi.StrategyBackgroundService.Dto.Services.Dto;

public class StrategyInfoDto
{
    public string Id { get; set; }

    public string StrategyType { get; set; }

    public string Symbol { get; set; }

    public string Timeframe { get; set; }

    public string Timeframe2 { get; set; }

    public string StrategyName { get; set; }

    public bool? CanRun { get; set; }

    public bool SecureControlPosition { get; set; }
    public string? Treshold { get; set; }

    public TickDto LastTick { get; set; }

    public CandleDto LastCandle { get; set; }
}