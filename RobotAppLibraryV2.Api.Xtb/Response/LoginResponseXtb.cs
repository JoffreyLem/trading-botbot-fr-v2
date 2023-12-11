using RobotAppLibraryV2.Modeles;

namespace RobotAppLibraryV2.Api.Xtb.Response;

public class LoginResponseXtb : LoginResponse
{
    public override string? StreamingSessionId { get; set; }
}