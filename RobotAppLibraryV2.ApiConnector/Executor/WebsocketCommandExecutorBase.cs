using RobotAppLibraryV2.ApiConnector.Connector.Websocket;
using RobotAppLibraryV2.ApiConnector.Interfaces;
using RobotAppLibraryV2.Modeles;

namespace RobotAppLibraryV2.ApiConnector.Executor;

public abstract class WebsocketCommandExecutorBase : ICommandExecutor
{
    protected readonly ICommandCreator CommandCreator;
    protected readonly IReponseAdapter ResponseAdapter;
    protected readonly WebsocketConnector WebsocketConnector;
    protected readonly WebsocketStreamingConnector WebsocketStreamingConnector;

    public WebsocketCommandExecutorBase(
        WebsocketConnector websocketConnector,
        WebsocketStreamingConnector websocketStreamingConnector,
        ICommandCreator commandCreator,
        IReponseAdapter responseAdapter
    )
    {
        CommandCreator = commandCreator;
        ResponseAdapter = responseAdapter;
        WebsocketConnector = websocketConnector;
        WebsocketStreamingConnector = websocketStreamingConnector;
        websocketConnector.Connected += (sender, args) => Connected?.Invoke(sender, args);
        websocketConnector.Disconnected += (sender, args) => Disconnected?.Invoke(sender, args);
        websocketStreamingConnector.Connected += (sender, args) => Connected?.Invoke(sender, args);
        websocketStreamingConnector.Disconnected += (sender, args) => Disconnected?.Invoke(sender, args);
        websocketStreamingConnector.TickRecordReceived += tick => TickRecordReceived?.Invoke(tick);
        websocketStreamingConnector.TradeRecordReceived += position => TradeRecordReceived?.Invoke(position);
        websocketStreamingConnector.BalanceRecordReceived += balance => BalanceRecordReceived?.Invoke(balance);
        websocketStreamingConnector.ProfitRecordReceived += profit => ProfitRecordReceived?.Invoke(profit);
        websocketStreamingConnector.NewsRecordReceived += news => NewsRecordReceived?.Invoke(news);
        websocketStreamingConnector.KeepAliveRecordReceived += () => KeepAliveRecordReceived?.Invoke();
        websocketStreamingConnector.CandleRecordReceived += candle => CandleRecordReceived?.Invoke(candle);
    }

    public event Action<Tick>? TickRecordReceived;
    public event Action<Position?>? TradeRecordReceived;
    public event Action<AccountBalance?>? BalanceRecordReceived;
    public event Action<Position>? ProfitRecordReceived;
    public event Action<News>? NewsRecordReceived;
    public event Action? KeepAliveRecordReceived;
    public event Action<Candle>? CandleRecordReceived;
    public event EventHandler? Connected;
    public event EventHandler? Disconnected;

    public void Dispose()
    {
        WebsocketConnector.Close();
        WebsocketStreamingConnector.Close();
    }

    public virtual async Task ExecuteLoginCommand(Credentials credentials)
    {
        await WebsocketConnector.ConnectAsync();
        var command = CommandCreator.CreateLoginCommand(credentials);
        await WebsocketConnector.SendAndReceiveAsync(command);
        await WebsocketStreamingConnector.ConnectAsync();
    }

    public async Task ExecuteLogoutCommand()
    {
        var command = CommandCreator.CreateLogOutCommand();
        await WebsocketConnector.SendAndReceiveAsync(command);
    }

    public virtual async Task<List<SymbolInfo>> ExecuteAllSymbolsCommand()
    {
        var command = CommandCreator.CreateAllSymbolsCommand();
        var rsp = await WebsocketConnector.SendAndReceiveAsync(command, false);
        return ResponseAdapter.AdaptAllSymbolsResponse(rsp);
    }

    public virtual async Task<List<CalendarData>> ExecuteCalendarCommand()
    {
        var command = CommandCreator.CreateCalendarCommand();
        var rsp = await WebsocketConnector.SendAndReceiveAsync(command);
        return ResponseAdapter.AdaptCalendarResponse(rsp);
    }

    public virtual async Task<List<Candle>> ExecuteFullChartCommand(Timeframe timeframe, DateTime start, string symbol)
    {
        var command = CommandCreator.CreateFullChartCommand(timeframe, start, symbol);
        var rsp = await WebsocketConnector.SendAndReceiveAsync(command, false);
        return ResponseAdapter.AdaptFullChartResponse(rsp);
    }

    public virtual async Task<List<Candle>> ExecuteRangeChartCommand(Timeframe timeframe, DateTime start, DateTime end,
        string symbol)
    {
        var command = CommandCreator.CreateRangeChartCommand(timeframe, start, end, symbol);
        var rsp = await WebsocketConnector.SendAndReceiveAsync(command, false);
        return ResponseAdapter.AdaptRangeChartResponse(rsp);
    }

    public virtual async Task<AccountBalance?> ExecuteBalanceAccountCommand()
    {
        var command = CommandCreator.CreateBalanceAccountCommand();
        var rsp = await WebsocketConnector.SendAndReceiveAsync(command);
        return ResponseAdapter.AdaptBalanceAccountResponse(rsp);
    }

    public virtual async Task<List<News>> ExecuteNewsCommand(DateTime? start, DateTime? end)
    {
        var command = CommandCreator.CreateNewsCommand(start, end);
        var rsp = await WebsocketConnector.SendAndReceiveAsync(command);
        return ResponseAdapter.AdaptNewsResponse(rsp);
    }

    public virtual async Task<string> ExecuteCurrentUserDataCommand()
    {
        var command = CommandCreator.CreateCurrentUserDataCommand();
        var rsp = await WebsocketConnector.SendAndReceiveAsync(command);
        return ResponseAdapter.AdaptCurrentUserDataResponse(rsp);
    }

    public virtual async Task<bool> ExecutePingCommand()
    {
        var command = CommandCreator.CreatePingCommand();
        var rsp = await WebsocketConnector.SendAndReceiveAsync(command);
        return ResponseAdapter.AdaptPingResponse(rsp);
    }

    public virtual async Task<SymbolInfo> ExecuteSymbolCommand(string symbol)
    {
        var command = CommandCreator.CreateSymbolCommand(symbol);
        var rsp = await WebsocketConnector.SendAndReceiveAsync(command);
        return ResponseAdapter.AdaptSymbolResponse(rsp);
    }

    public virtual async Task<Tick> ExecuteTickCommand(string symbol)
    {
        var command = CommandCreator.CreateTickCommand(symbol);
        var rsp = await WebsocketConnector.SendAndReceiveAsync(command);
        return ResponseAdapter.AdaptTickResponse(rsp);
    }


    public virtual async Task<List<Position>> ExecuteTradesHistoryCommand(string tradeCom)
    {
        var command = CommandCreator.CreateTradesHistoryCommand();
        var rsp = await WebsocketConnector.SendAndReceiveAsync(command, false);
        return ResponseAdapter.AdaptTradesHistoryResponse(rsp, tradeCom);
    }

    public virtual async Task<Position?> ExecuteTradesOpenedTradesCommand(string tradeCom)
    {
        var command = CommandCreator.CreateTradesOpenedTradesCommand();
        var rsp = await WebsocketConnector.SendAndReceiveAsync(command, false);
        return ResponseAdapter.AdaptTradesOpenedTradesResponse(rsp, tradeCom);
    }

    public virtual async Task<TradeHourRecord> ExecuteTradingHoursCommand(string symbol)
    {
        var command = CommandCreator.CreateTradingHoursCommand(symbol);
        var rsp = await WebsocketConnector.SendAndReceiveAsync(command);
        return ResponseAdapter.AdaptTradingHoursResponse(rsp);
    }

    public virtual async Task<Position> ExecuteOpenTradeCommand(Position position, decimal price)
    {
        var command = CommandCreator.CreateOpenTradeCommande(position, price);
        var rsp = await WebsocketConnector.SendAndReceiveAsync(command);
        return ResponseAdapter.AdaptOpenTradeResponse(rsp);
    }

    public virtual async Task<Position> ExecuteUpdateTradeCommand(Position position, decimal price)
    {
        var command = CommandCreator.CreateUpdateTradeCommande(position, price);
        var rsp = await WebsocketConnector.SendAndReceiveAsync(command);
        return ResponseAdapter.AdaptUpdateTradeResponse(rsp);
    }

    public virtual async Task<Position> ExecuteCloseTradeCommand(Position position, decimal price)
    {
        var command = CommandCreator.CreateCloseTradeCommande(position, price);
        var rsp = await WebsocketConnector.SendAndReceiveAsync(command);
        return ResponseAdapter.AdaptCloseTradeResponse(rsp);
    }

    public bool ExecuteIsConnected()
    {
        if (WebsocketConnector.IsConnected && WebsocketStreamingConnector.IsConnected) return true;

        return false;
    }

    public virtual async void ExecuteSubscribeBalanceCommandStreaming()
    {
        var command = CommandCreator.CreateSubscribeBalanceCommandStreaming();
        await WebsocketStreamingConnector.SendAsync(command);
    }

    public virtual async void ExecuteStopBalanceCommandStreaming()
    {
        var command = CommandCreator.CreateStopBalanceCommandStreaming();
        await WebsocketStreamingConnector.SendAsync(command);
    }

    public virtual async void ExecuteSubscribeCandleCommandStreaming(string symbol)
    {
        var command = CommandCreator.CreateSubscribeCandleCommandStreaming(symbol);
        await WebsocketStreamingConnector.SendAsync(command);
    }

    public virtual async void ExecuteStopCandleCommandStreaming(string symbol)
    {
        var command = CommandCreator.CreateStopCandleCommandStreaming(symbol);
        await WebsocketStreamingConnector.SendAsync(command);
    }

    public virtual async void ExecuteSubscribeKeepAliveCommandStreaming()
    {
        var command = CommandCreator.CreateSubscribeKeepAliveCommandStreaming();
        await WebsocketStreamingConnector.SendAsync(command);
    }

    public virtual async void ExecuteStopKeepAliveCommandStreaming()
    {
        var command = CommandCreator.CreateStopKeepAliveCommandStreaming();
        await WebsocketStreamingConnector.SendAsync(command);
    }

    public virtual async void ExecuteSubscribeNewsCommandStreaming()
    {
        var command = CommandCreator.CreateSubscribeNewsCommandStreaming();
        await WebsocketStreamingConnector.SendAsync(command);
    }

    public virtual async void ExecuteStopNewsCommandStreaming()
    {
        var command = CommandCreator.CreateStopNewsCommandStreaming();
        await WebsocketStreamingConnector.SendAsync(command);
    }

    public virtual async void ExecuteSubscribeProfitsCommandStreaming()
    {
        var command = CommandCreator.CreateSubscribeProfitsCommandStreaming();
        await WebsocketStreamingConnector.SendAsync(command);
    }

    public virtual async void ExecuteStopProfitsCommandStreaming()
    {
        var command = CommandCreator.CreateStopProfitsCommandStreaming();
        await WebsocketStreamingConnector.SendAsync(command);
    }

    public virtual async void ExecuteTickPricesCommandStreaming(string symbol)
    {
        var command = CommandCreator.CreateTickPricesCommandStreaming(symbol);
        await WebsocketStreamingConnector.SendAsync(command);
    }

    public virtual async void ExecuteStopTickPriceCommandStreaming(string symbol)
    {
        var command = CommandCreator.CreateStopTickPriceCommandStreaming(symbol);
        await WebsocketStreamingConnector.SendAsync(command);
    }

    public virtual async void ExecuteTradesCommandStreaming()
    {
        var command = CommandCreator.CreateTradesCommandStreaming();
        await WebsocketStreamingConnector.SendAsync(command);
    }

    public virtual async void ExecuteStopTradesCommandStreaming()
    {
        var command = CommandCreator.CreateStopTradesCommandStreaming();
        await WebsocketStreamingConnector.SendAsync(command);
    }

    public virtual async void ExecuteTradeStatusCommandStreaming()
    {
        var command = CommandCreator.CreateTradeStatusCommandStreaming();
        await WebsocketStreamingConnector.SendAsync(command);
    }

    public virtual async void ExecuteStopTradeStatusCommandStreaming()
    {
        var command = CommandCreator.CreateStopTradeStatusCommandStreaming();
        await WebsocketStreamingConnector.SendAsync(command);
    }

    public virtual async void ExecutePingCommandStreaming()
    {
        var command = CommandCreator.CreatePingCommandStreaming();
        await WebsocketStreamingConnector.SendAsync(command);
    }

    public virtual async void ExecuteStopPingCommandStreaming()
    {
        var command = CommandCreator.CreateStopPingCommandStreaming();
        await WebsocketStreamingConnector.SendAsync(command);
    }
}