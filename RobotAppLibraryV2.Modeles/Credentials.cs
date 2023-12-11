using Destructurama.Attributed;

namespace RobotAppLibraryV2.Modeles;

public class Credentials
{
    public string? User { get; set; }

    [LogMasked] public string? Password { get; set; }

    [LogMasked] public string? ApiKey { get; set; }
}