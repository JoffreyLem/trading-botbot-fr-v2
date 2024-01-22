using System.ComponentModel.DataAnnotations;
using RobotAppLibraryV2.Modeles;

namespace Robot.Server.Dto.Request;

public class StrategyInitDto
{
    [Required] public string StrategyFileId { get; set; }
    [Required] public string Symbol { get; set; }

    [Required] public Timeframe Timeframe { get; set; }
    [Required] public Timeframe Timeframe2 { get; set; }
}