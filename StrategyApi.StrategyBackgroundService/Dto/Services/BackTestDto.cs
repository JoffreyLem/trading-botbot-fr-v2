using RobotAppLibraryV2.Modeles;

namespace StrategyApi.StrategyBackgroundService.Dto.Services;

public class BackTestDto
{
    public bool IsBackTestRunning { get; set; }
    
    public DateTime? LastBackTestExecution { get; set; }
    
    public ResultDto? ResultBacktest { get; set; }
}