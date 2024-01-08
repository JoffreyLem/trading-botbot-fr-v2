using System.Text.RegularExpressions;
using RobotAppLibraryV2.ApiConnector.Exceptions;
using RobotAppLibraryV2.ApiConnector.Modeles;
using Serilog;

namespace RobotAppLibraryV2.ApiConnector.Connector.Websocket;

public class WebsocketConnector : WebSocketConnectorBase
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public WebsocketConnector(string serverUri, ILogger logger) : base(serverUri, logger)
    {
    }

    public async Task<string> SendAndReceiveAsync(string messageToSend, bool logResponse = true)
    {
        await _semaphore.WaitAsync();
        var tcpLog = new TcpLog
        {
            RequestMessage = FilterSensitiveData(messageToSend)
        };
        try
        {
            var currentTimestamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            var interval = currentTimestamp - lastCommandTimestamp;

            if (interval < CommandTimeSpanmeSpace.Ticks) await Task.Delay(CommandTimeSpanmeSpace);

            await SendAsync(messageToSend);
            lastCommandTimestamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            var response = await ReceiveAsync();

            var maskedResponse = logResponse ? FilterSensitiveData(response) : "Response not logged";

            tcpLog.ResponseMessage = maskedResponse;

            return response;
        }
        catch (Exception e)
        {
            Logger.Error(e, "Error on send and receive");
            throw new ApiCommunicationException("Error on API Communication");
        }
        finally
        {
            Logger.Information("Tcp log received : {@Tcp}", tcpLog);
            _semaphore.Release();
        }
    }

    private string FilterSensitiveData(string message)
    {
        message = MaskSensitiveData(message, "\"Password\":\".*?\"", "\"Password\":\"****\"");
        message = MaskSensitiveData(message, "\"ApiKey\":\".*?\"", "\"ApiKey\":\"****\"");
        return message;
    }

    private string MaskSensitiveData(string message, string pattern, string replacement)
    {
        return Regex.Replace(message, pattern, replacement);
    }
}