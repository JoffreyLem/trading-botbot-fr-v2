namespace RobotAppLibraryV2.ApiConnector.Tcp;

public interface ITcpConnectorBase
{
    void Dispose();
    event EventHandler? Connected;
    event EventHandler? Disconnected;
    Task ConnectAsync();
    Task SendAsync(string messageToSend);
    Task<string> ReceiveAsync(CancellationToken cancellationToken = default);
    void Close();
}