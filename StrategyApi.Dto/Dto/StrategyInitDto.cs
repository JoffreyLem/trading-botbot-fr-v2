using System.ComponentModel.DataAnnotations;
using StrategyApi.Dto.Enum;

namespace StrategyApi.Dto.Dto;

public class StrategyInitDto
{
    [Required] public StrategyTypeEnum StrategyType { get; set; }
    [Required] public string Symbol { get; set; }
    [Required] public string Timeframe { get; set; }

    [Required] public string Timeframe2 { get; set; }
}