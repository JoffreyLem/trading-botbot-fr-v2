using RobotAppLibraryV2.ApiConnector.Exceptions;
using RobotAppLibraryV2.ApiConnector.Modeles;
using Serilog;

namespace RobotAppLibraryV2.ApiConnector.Tcp;

public class TcpConnector : TcpClientWrapperBase, ITcpConnectorSynchronisation
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private long lastCommandTimestamp;

    public TcpConnector(Server server, ILogger logger) : base(server.Address, server.MainPort, logger)
    {
    }

    public async Task<string?> SendAndReceiveAsync(string messageToSend, bool logResponse = true)
    {
        await _semaphore.WaitAsync();
        var tcpLog = new TcpLog
        {
            RequestMessage = messageToSend
        };
        try
        {
            var currentTimestamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            var interval = currentTimestamp - lastCommandTimestamp;

            if (interval < CommandTImeSpace.Ticks) await Task.Delay(CommandTImeSpace);

            await SendAsync(messageToSend);
            lastCommandTimestamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            var response = await ReceiveAsync();
            if (logResponse)
                tcpLog.ResponseMessage = response;
            else
                tcpLog.ResponseMessage = "TO BIG FOR THE LOG :(";

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

    private string TruncateResponse(string response, int maxLength)
    {
        return response.Length <= maxLength ? response : response.Substring(0, maxLength) + "...";
    }
}