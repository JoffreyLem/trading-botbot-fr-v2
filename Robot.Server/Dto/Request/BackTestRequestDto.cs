namespace Robot.Server.Dto.Request;

public class BackTestRequestDto
{
    public double Balance { get; set; }
    public decimal MinSpread { get; set; }
    public decimal MaxSpread { get; set; }
}