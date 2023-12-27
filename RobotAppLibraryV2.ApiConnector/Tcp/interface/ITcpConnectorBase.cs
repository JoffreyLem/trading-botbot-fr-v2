namespace RobotAppLibraryV2.ApiConnector.Tcp.@interface;

public interface ITcpConnectorBase : ITcpEvent
{
    void Dispose();
    Task ConnectAsync();
    Task SendAsync(string messageToSend);
    Task<string> ReceiveAsync(CancellationToken cancellationToken = default);
    void Close();
}