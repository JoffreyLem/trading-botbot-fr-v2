namespace Robot.Server.Dto.Response;

public class BackTestDto
{
    public bool IsBackTestRunning { get; set; }

    public DateTime? LastBackTestExecution { get; set; }

    public ResultDto? ResultBacktest { get; set; }
}