namespace Robot.Server.Dto.Response;

public class StrategyCreatedResponseDto
{
    public bool Created { get; set; }

    public StrategyFileDto StrategyFile { get; set; }

    public List<string>? Errors { get; set; } = new();
}