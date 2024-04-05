namespace Robot.Server.Dto;

public class ApiResponseError
{

    public string? Error { get; set; }
    public List<string>? Errors { get; set; } = new();
}