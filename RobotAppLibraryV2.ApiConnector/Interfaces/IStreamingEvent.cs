using RobotAppLibraryV2.Modeles;

namespace RobotAppLibraryV2.ApiConnector.Interfaces;

public interface IStreamingEvent
{
    public event Action<Tick>? TickRecordReceived;


    public event Action<Position?>? TradeRecordReceived;


    public event Action<AccountBalance?>? BalanceRecordReceived;


    public event Action<Position>? ProfitRecordReceived;


    public event Action<News>? NewsRecordReceived;
    public event Action? KeepAliveRecordReceived;
    public event Action<Candle>? CandleRecordReceived;
}