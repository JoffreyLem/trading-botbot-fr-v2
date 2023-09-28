using RobotAppLibraryV2.Modeles;

namespace RobotAppLibraryV2.ApiHandler.Interfaces;

public interface IApiHandler
{
    public AccountBalance AccountBalance { get; set; }
    public event EventHandler Connected;
    public event EventHandler Disconnected;
    public event EventHandler<Tick> TickEvent;
    public event EventHandler<Position> PositionOpenedEvent;
    public event EventHandler<Position> PositionUpdatedEvent;
    public event EventHandler<Position> PositionRejectedEvent;
    public event EventHandler<Position> PositionClosedEvent;
    public event EventHandler<AccountBalance> NewBalanceEvent;
    public event EventHandler<News> NewsEvent;
    public Task ConnectAsync(string user, string pwd);
    public Task DisconnectAsync();
    public bool IsConnected();
    public Task PingAsync();
    public Task<AccountBalance> GetBalanceAsync();
    public Task<List<Position>> GetAllPositionsAsync();
    public Task<List<Calendar>> GetCalendarAsync();
    public Task<List<string>> GetAllSymbolsAsync();
    public Task<List<Position>> GetCurrentTradesAsync();
    public Task<List<Position>> GetAllPositionsByCommentAsync(string comment);
    public Task<SymbolInfo> GetSymbolInformationAsync(string symbol);
    public Task<TradeHourRecord> GetTradingHoursAsync(string symbol);
    public Task<List<Candle>> GetChartAsync(string symbol, Timeframe timeframe);

    public Task<List<Candle>> GetChartByDateAsync(string symbol, Timeframe periodCodeStr, DateTime start,
        DateTime end);

    public Task<Tick> GetTickPriceAsync(string symbol);
    public Task<Position> OpenPositionAsync(Position position);
    public Task UpdatePositionAsync(decimal price, Position position);
    public Task ClosePositionAsync(decimal price, Position position);
    public Task<bool> CheckIfSymbolExistAsync(string symbol);
    public void SubscribePrice(string symbol);

    public void UnsubscribePrice(string symbol);
}