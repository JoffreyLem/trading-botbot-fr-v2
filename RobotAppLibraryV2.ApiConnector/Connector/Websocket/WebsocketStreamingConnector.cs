using RobotAppLibraryV2.ApiConnector.Interfaces;
using RobotAppLibraryV2.Modeles;
using Serilog;

namespace RobotAppLibraryV2.ApiConnector.Connector.Websocket;

public abstract class WebsocketStreamingConnector : WebSocketConnectorBase, IStreamingEvent
{
    public WebsocketStreamingConnector(string serverUri, ILogger logger) : base(serverUri, logger)
    {
    }

    public event Action<Tick>? TickRecordReceived;
    public event Action<Position?>? TradeRecordReceived;
    public event Action<AccountBalance?>? BalanceRecordReceived;
    public event Action<Position>? ProfitRecordReceived;
    public event Action<News>? NewsRecordReceived;
    public event Action? KeepAliveRecordReceived;
    public event Action<Candle>? CandleRecordReceived;


    public override async Task ConnectAsync()
    {
        await base.ConnectAsync();

        var t = new Thread(async () =>
        {
            while (IsConnected) await ReadStreamMessage();
        });

        t.Start();
    }

    public override async Task SendAsync(string messageToSend)
    {
        var currentTimestamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

        var interval = currentTimestamp - lastCommandTimestamp;

        if (interval < CommandTimeSpanmeSpace.Ticks) await Task.Delay(CommandTimeSpanmeSpace);

        lastCommandTimestamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        Logger.Information("Streaming message to send {Message}", messageToSend);
        await base.SendAsync(messageToSend);
    }

    protected abstract void HandleMessage(string message);

    private async Task ReadStreamMessage()
    {
        try
        {
            var message = await ReceiveAsync();
            if (!string.IsNullOrEmpty(message))
            {
                Logger.Verbose("New stream message received {@message}", message);
                HandleMessage(message);
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, "Error on read stream message");
        }
    }

    protected virtual void OnTickRecordReceived(Tick obj)
    {
        Logger.Verbose("New tick event {@Tick}", obj);
        TickRecordReceived?.Invoke(obj);
    }

    protected virtual void OnTradeRecordReceived(Position? obj)
    {
        Logger.Information("Position event {@obj}", obj);
        TradeRecordReceived?.Invoke(obj);
    }

    protected virtual void OnBalanceRecordReceived(AccountBalance? obj)
    {
        Logger.Verbose("Account balance event {@obj}", obj);
        BalanceRecordReceived?.Invoke(obj);
    }


    protected virtual void OnProfitRecordReceived(Position obj)
    {
        Logger.Verbose("Profit record event {@obj}", obj);
        ProfitRecordReceived?.Invoke(obj);
    }

    protected virtual void OnNewsRecordReceived(News obj)
    {
        Logger.Verbose("News event {@obj}", obj);
        NewsRecordReceived?.Invoke(obj);
    }

    protected virtual void OnKeepAliveRecordReceived()
    {
        Logger.Verbose("Keep alive event");
        KeepAliveRecordReceived?.Invoke();
    }

    protected virtual void OnCandleRecordReceived(Candle obj)
    {
        Logger.Verbose("Candle event {@obj}", obj);
        CandleRecordReceived?.Invoke(obj);
    }
}