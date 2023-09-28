namespace RobotAppLibraryV2.Attributes;

public class VersionStrategyAttribute : Attribute
{
    public VersionStrategyAttribute(string version)
    {
        Version = version;
    }

    public string Version { get; set; }
}