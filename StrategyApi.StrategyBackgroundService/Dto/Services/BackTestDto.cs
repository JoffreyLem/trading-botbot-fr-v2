namespace StrategyApi.StrategyBackgroundService.Dto.Services;

public class BackTestDto
{
    public bool IsBackTestRunning { get; set; }
    
    public DateTime? LastBackTestExecution { get; set; }
}