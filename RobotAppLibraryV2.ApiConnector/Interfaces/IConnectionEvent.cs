namespace RobotAppLibraryV2.ApiConnector.Interfaces;

public interface IConnectionEvent
{
    event EventHandler? Connected;
    event EventHandler? Disconnected;
}