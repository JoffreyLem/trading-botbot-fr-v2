namespace RobotAppLibraryV2.ApiConnector.Tcp.@interface;

public interface ITcpConnectorSynchronisation : ITcpConnectorBase
{
    Task<string> SendAndReceiveAsync(string messageToSend, bool logResponse = true);
}