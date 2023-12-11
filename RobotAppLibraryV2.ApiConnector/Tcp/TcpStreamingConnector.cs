using RobotAppLibraryV2.ApiConnector.Modeles;
using RobotAppLibraryV2.Modeles;
using Serilog;

namespace RobotAppLibraryV2.ApiConnector.Tcp;

public abstract class TcpStreamingConnector : TcpClientWrapperBase, ITcpStreamingConnector
{
    public TcpStreamingConnector(Server server, ILogger logger) : base(server.Address, server.StreamingPort, logger)
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

    protected abstract void HandleMessage(string? message);

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
        Logger.Debug("New tick event {@Tick}", obj);
        TickRecordReceived?.Invoke(obj);
    }

    protected virtual void OnTradeRecordReceived(Position? obj)
    {
        Logger.Debug("Position event {@obj}", obj);
        TradeRecordReceived?.Invoke(obj);
    }

    protected virtual void OnBalanceRecordReceived(AccountBalance? obj)
    {
        Logger.Debug("Account balance event {@obj}", obj);
        BalanceRecordReceived?.Invoke(obj);
    }


    protected virtual void OnProfitRecordReceived(Position obj)
    {
        Logger.Debug("Profit record event {@obj}", obj);
        ProfitRecordReceived?.Invoke(obj);
    }

    protected virtual void OnNewsRecordReceived(News obj)
    {
        Logger.Debug("News event {@obj}", obj);
        NewsRecordReceived?.Invoke(obj);
    }

    protected virtual void OnKeepAliveRecordReceived()
    {
        Logger.Debug("Keep alive event");
        KeepAliveRecordReceived?.Invoke();
    }

    protected virtual void OnCandleRecordReceived(Candle obj)
    {
        Logger.Debug("Candle event {@obj}", obj);
        CandleRecordReceived?.Invoke(obj);
    }
}