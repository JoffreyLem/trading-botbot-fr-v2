using RobotAppLibraryV2.ApiConnector.Interfaces;
using RobotAppLibraryV2.Modeles;

namespace RobotAppLibraryV2.ApiConnector.Tcp;

public abstract class
    TcpCommandExecutorBase<TConnector, TStreamingConnector, TCommandCreator, TResponseAdapter> : ICommandExecutor
    where TConnector : TcpConnector
    where TStreamingConnector : TcpStreamingConnector
    where TCommandCreator : ICommandCreator
    where TResponseAdapter : IReponseAdapter
{
    protected readonly TResponseAdapter _responseAdapter;


    protected readonly TCommandCreator commandCreator;
    protected readonly TConnector tcpClient;
    protected readonly TStreamingConnector tcpStreamingClient;


    protected TcpCommandExecutorBase(TConnector tcpClient, TStreamingConnector tcpStreamingClient,
        TCommandCreator commandCreator, TResponseAdapter responseAdapter)
    {
        this.tcpClient = tcpClient;
        this.tcpStreamingClient = tcpStreamingClient;
        this.commandCreator = commandCreator;
        _responseAdapter = responseAdapter;
    }


    public virtual async Task ExecuteLoginCommand(Credentials credentials)
    {
        var command = commandCreator.CreateLoginCommand(credentials);
        await tcpClient.SendAndReceiveAsync(command);
    }

    public async Task ExecuteLogoutCommand()
    {
        var command = commandCreator.CreateLogOutCommand();
        await tcpClient.SendAndReceiveAsync(command);
    }

    public virtual async Task<List<SymbolInfo>> ExecuteAllSymbolsCommand()
    {
        var command = commandCreator.CreateAllSymbolsCommand();
        var rsp = await tcpClient.SendAndReceiveAsync(command, false);
        return _responseAdapter.AdaptAllSymbolsResponse(rsp);
    }

    public virtual async Task<List<CalendarData>> ExecuteCalendarCommand()
    {
        var command = commandCreator.CreateCalendarCommand();
        var rsp = await tcpClient.SendAndReceiveAsync(command);
        return _responseAdapter.AdaptCalendarResponse(rsp);
    }

    public virtual async Task<List<Candle>> ExecuteFullChartCommand(Timeframe timeframe, DateTime start, string symbol)
    {
        var command = commandCreator.CreateFullChartCommand(timeframe, start, symbol);
        var rsp = await tcpClient.SendAndReceiveAsync(command, false);
        return _responseAdapter.AdaptFullChartResponse(rsp);
    }

    public virtual async Task<List<Candle>> ExecuteRangeChartCommand(Timeframe timeframe, DateTime start, DateTime end,
        string symbol)
    {
        var command = commandCreator.CreateRangeChartCommand(timeframe, start, end, symbol);
        var rsp = await tcpClient.SendAndReceiveAsync(command, false);
        return _responseAdapter.AdaptRangeChartResponse(rsp);
    }

    public virtual async Task<AccountBalance?> ExecuteBalanceAccountCommand()
    {
        var command = commandCreator.CreateBalanceAccountCommand();
        var rsp = await tcpClient.SendAndReceiveAsync(command);
        return _responseAdapter.AdaptBalanceAccountResponse(rsp);
    }

    public virtual async Task<List<News>> ExecuteNewsCommand(DateTime? start, DateTime? end)
    {
        var command = commandCreator.CreateNewsCommand(start, end);
        var rsp = await tcpClient.SendAndReceiveAsync(command);
        return _responseAdapter.AdaptNewsResponse(rsp);
    }

    public virtual async Task<string> ExecuteCurrentUserDataCommand()
    {
        var command = commandCreator.CreateCurrentUserDataCommand();
        var rsp = await tcpClient.SendAndReceiveAsync(command);
        return _responseAdapter.AdaptCurrentUserDataResponse(rsp);
    }

    public virtual async Task<bool> ExecutePingCommand()
    {
        var command = commandCreator.CreatePingCommand();
        var rsp = await tcpClient.SendAndReceiveAsync(command);
        return _responseAdapter.AdaptPingResponse(rsp);
    }

    public virtual async Task<SymbolInfo> ExecuteSymbolCommand(string symbol)
    {
        var command = commandCreator.CreateSymbolCommand(symbol);
        var rsp = await tcpClient.SendAndReceiveAsync(command);
        return _responseAdapter.AdaptSymbolResponse(rsp);
    }

    public virtual async Task<Tick> ExecuteTickCommand(string symbol)
    {
        var command = commandCreator.CreateTickCommand(symbol);
        var rsp = await tcpClient.SendAndReceiveAsync(command);
        return _responseAdapter.AdaptTickResponse(rsp);
    }


    public virtual async Task<List<Position?>> ExecuteTradesHistoryCommand(string tradeCom)
    {
        var command = commandCreator.CreateTradesHistoryCommand();
        var rsp = await tcpClient.SendAndReceiveAsync(command, false);
        return _responseAdapter.AdaptTradesHistoryResponse(rsp, tradeCom);
    }

    public virtual async Task<Position?> ExecuteTradesOpenedTradesCommand(string tradeCom)
    {
        var command = commandCreator.CreateTradesOpenedTradesCommand();
        var rsp = await tcpClient.SendAndReceiveAsync(command, false);
        return _responseAdapter.AdaptTradesOpenedTradesResponse(rsp, tradeCom);
    }

    public virtual async Task<TradeHourRecord> ExecuteTradingHoursCommand(string symbol)
    {
        var command = commandCreator.CreateTradingHoursCommand(symbol);
        var rsp = await tcpClient.SendAndReceiveAsync(command);
        return _responseAdapter.AdaptTradingHoursResponse(rsp);
    }

    public virtual async Task<Position> ExecuteOpenTradeCommand(Position position, decimal price)
    {
        var command = commandCreator.CreateOpenTradeCommande(position, price);
        var rsp = await tcpClient.SendAndReceiveAsync(command);
        return _responseAdapter.AdaptOpenTradeResponse(rsp);
    }

    public virtual async Task<Position> ExecuteUpdateTradeCommand(Position position, decimal price)
    {
        var command = commandCreator.CreateUpdateTradeCommande(position, price);
        var rsp = await tcpClient.SendAndReceiveAsync(command);
        return _responseAdapter.AdaptUpdateTradeResponse(rsp);
    }

    public virtual async Task<Position> ExecuteCloseTradeCommand(Position position, decimal price)
    {
        var command = commandCreator.CreateCloseTradeCommande(position, price);
        var rsp = await tcpClient.SendAndReceiveAsync(command);
        return _responseAdapter.AdaptCloseTradeResponse(rsp);
    }

    public bool ExecuteIsConnected()
    {
        if (tcpClient.IsConnected && tcpStreamingClient.IsConnected) return true;

        return false;
    }

    public virtual async void ExecuteSubscribeBalanceCommandStreaming()
    {
        var command = commandCreator.CreateSubscribeBalanceCommandStreaming();
        await tcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteStopBalanceCommandStreaming()
    {
        var command = commandCreator.CreateStopBalanceCommandStreaming();
        await tcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteSubscribeCandleCommandStreaming(string symbol)
    {
        var command = commandCreator.CreateSubscribeCandleCommandStreaming(symbol);
        await tcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteStopCandleCommandStreaming(string symbol)
    {
        var command = commandCreator.CreateStopCandleCommandStreaming(symbol);
        await tcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteSubscribeKeepAliveCommandStreaming()
    {
        var command = commandCreator.CreateSubscribeKeepAliveCommandStreaming();
        await tcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteStopKeepAliveCommandStreaming()
    {
        var command = commandCreator.CreateStopKeepAliveCommandStreaming();
        await tcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteSubscribeNewsCommandStreaming()
    {
        var command = commandCreator.CreateSubscribeNewsCommandStreaming();
        await tcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteStopNewsCommandStreaming()
    {
        var command = commandCreator.CreateStopNewsCommandStreaming();
        await tcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteSubscribeProfitsCommandStreaming()
    {
        var command = commandCreator.CreateSubscribeProfitsCommandStreaming();
        await tcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteStopProfitsCommandStreaming()
    {
        var command = commandCreator.CreateStopProfitsCommandStreaming();
        await tcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteTickPricesCommandStreaming(string symbol)
    {
        var command = commandCreator.CreateTickPricesCommandStreaming(symbol);
        await tcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteStopTickPriceCommandStreaming(string symbol)
    {
        var command = commandCreator.CreateStopTickPriceCommandStreaming(symbol);
        await tcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteTradesCommandStreaming()
    {
        var command = commandCreator.CreateTradesCommandStreaming();
        await tcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteStopTradesCommandStreaming()
    {
        var command = commandCreator.CreateStopTradesCommandStreaming();
        await tcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteTradeStatusCommandStreaming()
    {
        var command = commandCreator.CreateTradeStatusCommandStreaming();
        await tcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteStopTradeStatusCommandStreaming()
    {
        var command = commandCreator.CreateStopTradeStatusCommandStreaming();
        await tcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecutePingCommandStreaming()
    {
        var command = commandCreator.CreatePingCommand();
        await tcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteStopPingCommandStreaming()
    {
        var command = commandCreator.CreateStopPingCommandStreaming();
        await tcpStreamingClient.SendAsync(command);
    }


    public ITcpStreamingConnector TcpStreamingConnector => tcpStreamingClient;
    public ITcpConnectorSynchronisation TcpConnector => tcpClient;

    public void Dispose()
    {
        tcpStreamingClient.Close();
        tcpClient.Close();
    }
}