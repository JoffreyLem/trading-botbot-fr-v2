using System.ComponentModel.DataAnnotations;
using RobotAppLibraryV2.Modeles;
using StrategyApi.StrategyBackgroundService.Dto.Services.Enum;

namespace StrategyApi.StrategyBackgroundService.Dto.Services;

public class StrategyInitDto
{
    [Required] public StrategyTypeEnum StrategyType { get; set; }
    [Required] public string Symbol { get; set; }

    [Required] public Timeframe Timeframe { get; set; }
    [Required] public Timeframe Timeframe2 { get; set; }
}