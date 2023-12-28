namespace RobotAppLibraryV2.ApiConnector.Tcp.@interface;

public interface ITcpEvent
{
    event EventHandler? Connected;
    event EventHandler? Disconnected;
}