namespace Robot.Server.Dto.Response;

public class StrategyCreatedResponseDto
{
    public bool Created { get; set; }

    public List<string>? Errors { get; set; } = new();
}