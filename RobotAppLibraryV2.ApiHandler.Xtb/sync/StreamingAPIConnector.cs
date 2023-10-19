using System.Net.Security;
using System.Net.Sockets;
using RobotAppLibraryV2.ApiHandler.Xtb.errors;
using RobotAppLibraryV2.ApiHandler.Xtb.records;
using RobotAppLibraryV2.ApiHandler.Xtb.responses;
using RobotAppLibraryV2.ApiHandler.Xtb.streaming;
using RobotAppLibraryV2.ApiHandler.Xtb.utils;
using JSONObject = Newtonsoft.Json.Linq.JObject;

namespace RobotAppLibraryV2.ApiHandler.Xtb.sync;

public class StreamingAPIConnector : Connector, IStreamingApiConnector, IDisposable
{
    /// <summary>
    ///     True if streaming is running
    /// </summary>
    [Obsolete("Used only in older method")]
    private readonly bool running;

    /// <summary>
    ///     Dedicated streaming listener.
    /// </summary>
    private StreamingListener sl;

    /// <summary>
    ///     Stream session id (given on login).
    /// </summary>
    private string streamSessionId;
    
    private delegate void RecordReceived<T>(JSONObject data) where T : new();

    private Dictionary<string, RecordReceived<object>> _commandMap;

    /// <summary>
    ///     Creates new StreamingAPIConnector instance based on given server data.
    /// </summary>
    /// <param name="server">Server data</param>
    public StreamingAPIConnector(Server server)
    {
        this.server = server;
        apiConnected = false;
        OnDisconnected += () => OnDisconnectedStreamingCallback?.Invoke();
        
    }

    /// <summary>
    ///     Creates new StreamingAPIConnector instance based on given server data, stream session id and streaming listener.
    /// </summary>
    /// <param name="server">Server data</param>
    public StreamingAPIConnector(Server server, string streamSessionId, StreamingListener streamingListner)
    {
        this.server = server;
        this.streamSessionId = streamSessionId;
        OnDisconnected += () => OnDisconnectedStreamingCallback?.Invoke();
        Connect(streamingListner, streamSessionId);
    }

    /// <summary>
    ///     Creates new StreamingAPIConnector object.
    /// </summary>
    /// <param name="sl">Streaming listener</param>
    /// <param name="ip">IP address</param>
    /// <param name="port">Streaming port</param>
    /// <param name="lr">Login response</param>
    /// <param name="secure">Secure</param>
    [Obsolete("Use StreamingAPIConnector(Server server) instead")]
    private StreamingAPIConnector(StreamingListener sl, string ip, int port, LoginResponse lr, bool secure)
    {
        running = true;
        this.sl = sl;
        streamSessionId = lr.StreamSessionId;
        apiSocket = new TcpClient(ip, port);

        if (secure)
        {
            var ssl = new SslStream(apiSocket.GetStream());
            ssl.AuthenticateAsClient(ip);
            apiWriteStream = new StreamWriter(ssl);
            apiReadStream = new StreamReader(ssl);
        }
        else
        {
            var ns = apiSocket.GetStream();
            apiWriteStream = new StreamWriter(ns);
            apiReadStream = new StreamReader(ns);
        }

        var t = new Thread(delegate()
        {
            while (running)
            {
                ReadStreamMessage();
                Thread.Sleep(50);
            }
        });
        t.Start();
    }

    /// <summary>
    ///     Connect to the streaming.
    /// </summary>
    public void Connect()
    {
        Connect(null, streamSessionId);
    }

    /// <summary>
    ///     Stream session id (member of login response). Should be set after the successful login.
    /// </summary>
    public string StreamSessionId
    {
        get => streamSessionId;
        set => streamSessionId = value;
    }

    public void SubscribePrice(string symbol, long? minArrivalTime = null, long? maxLevel = null)
    {
        var tickPricesSubscribe =
            new TickPricesSubscribe(symbol, streamSessionId, minArrivalTime, maxLevel);
        WriteMessage(tickPricesSubscribe.ToString());
    }

    public void UnsubscribePrice(string symbol)
    {
        var tickPricesStop = new TickPricesStop(symbol);
        WriteMessage(tickPricesStop.ToString());
    }

    public void SubscribeTrades()
    {
        var tradeRecordsSubscribe = new TradeRecordsSubscribe(streamSessionId);
        WriteMessage(tradeRecordsSubscribe.ToString());
    }

    public void UnsubscribeTrades()
    {
        var tradeRecordsStop = new TradeRecordsStop();
        WriteMessage(tradeRecordsStop.ToString());
    }

    public void SubscribeBalance()
    {
        var balanceRecordsSubscribe = new BalanceRecordsSubscribe(streamSessionId);
        WriteMessage(balanceRecordsSubscribe.ToString());
    }

    public void UnsubscribeBalance()
    {
        var balanceRecordsStop = new BalanceRecordsStop();
        WriteMessage(balanceRecordsStop.ToString());
    }

    public void SubscribeTradeStatus()
    {
        var tradeStatusRecordsSubscribe = new TradeStatusRecordsSubscribe(streamSessionId);
        WriteMessage(tradeStatusRecordsSubscribe.ToString());
    }

    public void UnsubscribeTradeStatus()
    {
        var tradeStatusRecordsStop = new TradeStatusRecordsStop();
        WriteMessage(tradeStatusRecordsStop.ToString());
    }

    public void SubscribeProfits()
    {
        var profitsSubscribe = new ProfitsSubscribe(streamSessionId);
        WriteMessage(profitsSubscribe.ToString());
    }

    public void UnsubscribeProfits()
    {
        var profitsStop = new ProfitsStop();
        WriteMessage(profitsStop.ToString());
    }

    public void SubscribeNews()
    {
        var newsSubscribe = new NewsSubscribe(streamSessionId);
        WriteMessage(newsSubscribe.ToString());
    }

    public void UnsubscribeNews()
    {
        var newsStop = new NewsStop();
        WriteMessage(newsStop.ToString());
    }

    public void SubscribeKeepAlive()
    {
        var keepAliveSubscribe = new KeepAliveSubscribe(streamSessionId);
        WriteMessage(keepAliveSubscribe.ToString());
    }

    /// <summary>
    ///     Connect to the streaming using given streaming listener.
    /// </summary>
    /// <param name="streamingListener">Streaming listener</param>
    public void Connect(StreamingListener streamingListener)
    {
        Connect(streamingListener, streamSessionId);
    }

    /// <summary>
    ///     Connect to the streaming using given streaming listener.
    /// </summary>
    /// <param name="streamingListener">Streaming listener</param>
    /// <param name="streamSessionId">Stream session id</param>
    public void Connect(StreamingListener streamingListener, string streamSessionId)
    {
        this.streamSessionId = streamSessionId;

        if (this.streamSessionId == null) throw new APICommunicationException("please login first");

        if (Connected()) throw new APICommunicationException("stream already connected");

        sl = streamingListener;

        apiSocket = new TcpClient(server.Address, server.StreamingPort);
        apiConnected = true;

        if (OnConnected != null) OnConnected.Invoke(server);

        if (server.Secure)
        {
            var ssl = new SslStream(apiSocket.GetStream(), false, SSLHelper.TrustAllCertificatesCallback);
            ssl.AuthenticateAsClient(server.Address);
            apiWriteStream = new StreamWriter(ssl);
            apiReadStream = new StreamReader(ssl);
        }
        else
        {
            var ns = apiSocket.GetStream();
            apiWriteStream = new StreamWriter(ns);
            apiReadStream = new StreamReader(ns);
        }

        CancellationTokenSource cts = new CancellationTokenSource();

        Task.Run(() => 
        {
            while (!cts.Token.IsCancellationRequested && Connected())
            {
                ReadStreamMessage();
            }
        }, cts.Token);

    }


    /// <summary>
    ///     Reads stream message.
    /// </summary>
    private void ReadStreamMessage()
    {
        try
        {
            var message = ReadMessage();

            if (message != null)
            {
                var responseBody = JSONObject.Parse(message);
                var commandName = responseBody["command"].ToString();

                if (commandName == "tickPrices")
                {
                    var tickRecord = new StreamingTickRecord();
                    tickRecord.FieldsFromJSONObject((JSONObject)responseBody["data"]);

                    if (TickRecordReceived != null) TickRecordReceived.Invoke(tickRecord);

                    if (sl != null) sl.ReceiveTickRecord(tickRecord);
                }
                else if (commandName == "trade")
                {
                    var tradeRecord = new StreamingTradeRecord();
                    tradeRecord.FieldsFromJSONObject((JSONObject)responseBody["data"]);

                    if (TradeRecordReceived != null) TradeRecordReceived.Invoke(tradeRecord);

                    if (sl != null) sl.ReceiveTradeRecord(tradeRecord);
                }
                else if (commandName == "balance")
                {
                    var balanceRecord = new StreamingBalanceRecord();
                    balanceRecord.FieldsFromJSONObject((JSONObject)responseBody["data"]);

                    if (BalanceRecordReceived != null) BalanceRecordReceived.Invoke(balanceRecord);

                    if (sl != null) sl.ReceiveBalanceRecord(balanceRecord);
                }
                else if (commandName == "tradeStatus")
                {
                    var tradeStatusRecord = new StreamingTradeStatusRecord();
                    tradeStatusRecord.FieldsFromJSONObject((JSONObject)responseBody["data"]);

                    if (TradeStatusRecordReceived != null) TradeStatusRecordReceived.Invoke(tradeStatusRecord);

                    if (sl != null) sl.ReceiveTradeStatusRecord(tradeStatusRecord);
                }
                else if (commandName == "profit")
                {
                    var profitRecord = new StreamingProfitRecord();
                    profitRecord.FieldsFromJSONObject((JSONObject)responseBody["data"]);

                    if (ProfitRecordReceived != null) ProfitRecordReceived.Invoke(profitRecord);

                    if (sl != null) sl.ReceiveProfitRecord(profitRecord);
                }
                else if (commandName == "news")
                {
                    var newsRecord = new StreamingNewsRecord();
                    newsRecord.FieldsFromJSONObject((JSONObject)responseBody["data"]);

                    if (NewsRecordReceived != null) NewsRecordReceived.Invoke(newsRecord);

                    if (sl != null) sl.ReceiveNewsRecord(newsRecord);
                }
                else if (commandName == "keepAlive")
                {
                    var keepAliveRecord = new StreamingKeepAliveRecord();
                    keepAliveRecord.FieldsFromJSONObject((JSONObject)responseBody["data"]);

                    if (KeepAliveRecordReceived != null) KeepAliveRecordReceived.Invoke(keepAliveRecord);

                    if (sl != null) sl.ReceiveKeepAliveRecord(keepAliveRecord);
                }
                else if (commandName == "candle")
                {
                    var candleRecord = new StreamingCandleRecord();
                    candleRecord.FieldsFromJSONObject((JSONObject)responseBody["data"]);

                    if (CandleRecordReceived != null) CandleRecordReceived.Invoke(candleRecord);

                    if (sl != null) sl.ReceiveCandleRecord(candleRecord);
                }
                else
                {
                    throw new APICommunicationException("Unknown streaming record received");
                }
            }
        }
        catch (Exception)
        {
        }
    }

    public void SubscribePrices(IEnumerable<string> symbols)
    {
        foreach (var symbol in symbols) SubscribePrice(symbol);
    }

    public void UnsubscribePrices(LinkedList<string> symbols)
    {
        foreach (var symbol in symbols) UnsubscribePrice(symbol);
    }

    [Obsolete("Use SubscribeTradeStatus instead")]
    public void SubscribeReqStatus()
    {
        SubscribeTradeStatus();
    }

    [Obsolete("Use UnsubscribeTradeStatus instead")]
    public void UnsubscribeReqStatus()
    {
        var reqStatusRecordsStop = new TradeStatusRecordsStop();
        WriteMessage(reqStatusRecordsStop.ToString());
    }

    public void UnsubscribeKeepAlive()
    {
        var keepAliveStop = new KeepAliveStop();
        WriteMessage(keepAliveStop.ToString());
    }

    public void SubscribeCandles(string symbol)
    {
        var candleRecordsSubscribe = new CandleRecordsSubscribe(symbol, streamSessionId);
        WriteMessage(candleRecordsSubscribe.ToString());
    }

    public void UnsubscribeCandles(string symbol)
    {
        var candleRecordsStop = new CandleRecordsStop(symbol);
        WriteMessage(candleRecordsStop.ToString());
    }

    #region Events

    /// <summary>
    ///     Delegate called on connection establish.
    /// </summary>
    /// <param name="server">Server that the connection was made to</param>
    public delegate void OnConnectedCallback(Server server);

    /// <summary>
    ///     Event raised when connection is established.
    /// </summary>
    public event OnConnectedCallback OnConnected;

    public event OnDisconnectCallback? OnDisconnectedStreamingCallback;


    //public delegate void OnReceiveRe

    /// <summary>
    ///     Delegate called on tick record arrival.
    /// </summary>
    /// <param name="tickRecord">Received tick record</param>
    public delegate void OnTick(StreamingTickRecord tickRecord);

    /// <summary>
    ///     Event raised when tick is received.
    /// </summary>
    public event OnTick TickRecordReceived;

    /// <summary>
    ///     Delegate called on trade record arrival.
    /// </summary>
    /// <param name="tradeRecord">Received trade record</param>
    public delegate void OnTrade(StreamingTradeRecord tradeRecord);

    /// <summary>
    ///     Event raised when trade record is received.
    /// </summary>
    public event OnTrade TradeRecordReceived;

    /// <summary>
    ///     Delegate called on balance record arrival.
    /// </summary>
    /// <param name="balanceRecord">Received balance record</param>
    public delegate void OnBalance(StreamingBalanceRecord balanceRecord);

    /// <summary>
    ///     Event raised when balance record is received.
    /// </summary>
    public event OnBalance BalanceRecordReceived;

    /// <summary>
    ///     Delegate called on trade status record arrival.
    /// </summary>
    /// <param name="tradeStatusRecord">Received trade status record</param>
    public delegate void OnTradeStatus(StreamingTradeStatusRecord tradeStatusRecord);

    /// <summary>
    ///     Event raised when trade status record is received.
    /// </summary>
    public event OnTradeStatus TradeStatusRecordReceived;

    /// <summary>
    ///     Delegate called on profit record arrival.
    /// </summary>
    /// <param name="profitRecord">Received profit record</param>
    public delegate void OnProfit(StreamingProfitRecord profitRecord);

    /// <summary>
    ///     Event raised when profit record is received.
    /// </summary>
    public event OnProfit ProfitRecordReceived;

    /// <summary>
    ///     Delegate called on news record arrival.
    /// </summary>
    /// <param name="newsRecord">Received news record</param>
    public delegate void OnNews(StreamingNewsRecord newsRecord);

    /// <summary>
    ///     Event raised when news record is received.
    /// </summary>
    public event OnNews NewsRecordReceived;

    /// <summary>
    ///     Delegate called on keep alive record arrival.
    /// </summary>
    /// <param name="keepAliveRecord">Received keep alive record</param>
    public delegate void OnKeepAlive(StreamingKeepAliveRecord keepAliveRecord);

    /// <summary>
    ///     Event raised when keep alive record is received.
    /// </summary>
    public event OnKeepAlive KeepAliveRecordReceived;

    /// <summary>
    ///     Delegate called on candle record arrival.
    /// </summary>
    /// <param name="candleRecord">Received candle record</param>
    public delegate void OnCandle(StreamingCandleRecord candleRecord);

    /// <summary>
    ///     Event raised when candle record is received.
    /// </summary>
    public event OnCandle CandleRecordReceived;

    #endregion
}