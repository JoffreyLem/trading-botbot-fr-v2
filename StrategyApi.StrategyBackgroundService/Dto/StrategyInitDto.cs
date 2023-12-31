using System.ComponentModel.DataAnnotations;
using RobotAppLibraryV2.Modeles;

namespace StrategyApi.StrategyBackgroundService.Dto;

public class StrategyInitDto
{
    [Required] public StrategyFileDto StrategyFileDto { get; set; }
    [Required] public string Symbol { get; set; }

    [Required] public Timeframe Timeframe { get; set; }
    [Required] public Timeframe Timeframe2 { get; set; }
}