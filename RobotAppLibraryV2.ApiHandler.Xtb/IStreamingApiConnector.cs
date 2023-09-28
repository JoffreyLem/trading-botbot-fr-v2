using RobotAppLibraryV2.ApiHandler.Xtb.sync;

namespace RobotAppLibraryV2.ApiHandler.Xtb;

public interface IStreamingApiConnector : IConnector
{
    public string StreamSessionId { get; set; }
    void Connect();

    void SubscribePrice(string symbol, long? minArrivalTime = null, long? maxLevel = null);

    void UnsubscribePrice(string symbol);

    void SubscribeTrades();

    void UnsubscribeTrades();

    void SubscribeBalance();

    void UnsubscribeBalance();

    void SubscribeTradeStatus();

    void UnsubscribeTradeStatus();

    void SubscribeProfits();

    void UnsubscribeProfits();

    void SubscribeKeepAlive();

    public void SubscribeNews();

    public void UnsubscribeNews();

    public event StreamingAPIConnector.OnConnectedCallback OnConnected;

    public event Connector.OnDisconnectCallback OnDisconnectedStreamingCallback;

    public event StreamingAPIConnector.OnTick TickRecordReceived;

    public event StreamingAPIConnector.OnTrade TradeRecordReceived;

    public event StreamingAPIConnector.OnBalance BalanceRecordReceived;

    public event StreamingAPIConnector.OnTradeStatus TradeStatusRecordReceived;

    public event StreamingAPIConnector.OnProfit ProfitRecordReceived;

    public event StreamingAPIConnector.OnNews NewsRecordReceived;
}