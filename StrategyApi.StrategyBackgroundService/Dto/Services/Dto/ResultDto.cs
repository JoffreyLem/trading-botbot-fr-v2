namespace StrategyApi.StrategyBackgroundService.Dto.Services.Dto;

public class ResultDto
{
    public decimal DrawndownMax { get; set; }
    public decimal GainMax { get; set; }

    public decimal MoyenneNegative { get; set; }
    public decimal MoyennePositive { get; set; }
    public decimal MoyenneProfit { get; set; }
    public decimal PerteMax { get; set; }

    public decimal Profit { get; set; }
    public decimal ProfitFactor { get; set; }
    public decimal ProfitNegatif { get; set; }
    public decimal ProfitPositif { get; set; }
    public decimal RatioMoyennePositifNegatif { get; set; }
    public decimal TauxReussite { get; set; }
    public double TotalPositionNegative { get; set; }
    public double TotalPositionPositive { get; set; }
    public double TotalPositions { get; set; }
}