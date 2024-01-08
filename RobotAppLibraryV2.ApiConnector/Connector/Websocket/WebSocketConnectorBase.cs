using System.Net.WebSockets;
using System.Text;
using RobotAppLibraryV2.ApiConnector.Exceptions;
using RobotAppLibraryV2.ApiConnector.Interfaces;
using Serilog;

namespace RobotAppLibraryV2.ApiConnector.Connector.Websocket;

public class WebSocketConnectorBase : IConnectorBase
{
    private readonly Uri _serverUri;
    private readonly ClientWebSocket _webSocket;
    protected readonly ILogger Logger;

    protected TimeSpan CommandTimeSpanmeSpace = TimeSpan.FromMilliseconds(200);
    protected long lastCommandTimestamp;

    public WebSocketConnectorBase(string serverUri, ILogger logger)
    {
        Logger = logger;
        _webSocket = new ClientWebSocket();
        _serverUri = new Uri(serverUri);
    }

    public bool IsConnected => _webSocket.State == WebSocketState.Open;

    public event EventHandler? Connected;
    public event EventHandler? Disconnected;

    public virtual async Task ConnectAsync()
    {
        if (_webSocket.State != WebSocketState.Open)
        {
            await _webSocket.ConnectAsync(_serverUri, CancellationToken.None);
            OnConnected(EventArgs.Empty);
        }
    }

    public virtual async Task SendAsync(string messageToSend)
    {
        try
        {
            if (_webSocket.State != WebSocketState.Open)
                throw new InvalidOperationException("Cannot send message. WebSocket is not connected.");

            var buffer = Encoding.UTF8.GetBytes(messageToSend);
            await _webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true,
                CancellationToken.None);
        }
        catch (WebSocketException ex)
        {
            Logger.Error(ex, "Web socket exception error occured");
            throw new ApiCommunicationException("Cannot send message.");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error on send");
            throw new ApiCommunicationException("Error on send");
        }
    }

    public virtual async Task<string> ReceiveAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_webSocket.State != WebSocketState.Open)
                throw new ApiCommunicationException("Cannot receive message. WebSocket is not connected.");

            const int bufferSize = 1024;
            var buffer = new byte[bufferSize];
            var result = new StringBuilder();

            WebSocketReceiveResult receiveResult;
            do
            {
                receiveResult = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                var messageFragment = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
                result.Append(messageFragment);
            } while (!receiveResult.EndOfMessage);

            return result.ToString();
        }
        catch (WebSocketException ex)
        {
            Logger.Error(ex, "Error while receiving data: ");
            throw new ApiCommunicationException("Error while receiving data: ");
        }
        catch (OperationCanceledException ex)
        {
            Logger.Error(ex, "Receiving data was canceled: ");
            throw new ApiCommunicationException("Receiving data was canceled: ");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Unexpected error occurred: ");
            throw new ApiCommunicationException("Unexpected error occurred: ");
        }
    }


    public void Close()
    {
        if (_webSocket != null && _webSocket.State == WebSocketState.Open)
        {
            _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).Wait();
            OnDisconnected(EventArgs.Empty);
        }
    }

    public void Dispose()
    {
        if (_webSocket != null) _webSocket.Dispose();
    }

    protected virtual void OnConnected(EventArgs e)
    {
        Connected?.Invoke(this, e);
    }

    protected virtual void OnDisconnected(EventArgs e)
    {
        Disconnected?.Invoke(this, e);
    }
}