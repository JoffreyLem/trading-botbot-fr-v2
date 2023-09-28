using RobotAppLibraryV2.ApiHandler.Xtb.codes;
using RobotAppLibraryV2.ApiHandler.Xtb.records;
using RobotAppLibraryV2.ApiHandler.Xtb.responses;
using RobotAppLibraryV2.ApiHandler.Xtb.sync;

namespace RobotAppLibraryV2.ApiHandler.Xtb;

public interface IApiCommandFactory
{
    public LoginResponse ExecuteLoginCommand(ISyncApiConnector connector, Credentials credentials,
        bool prettyPrint = false);

    public PingResponse ExecutePingCommand(ISyncApiConnector connector, bool prettyPrint = false);

    public MarginLevelResponse ExecuteMarginLevelCommand(ISyncApiConnector connector,
        bool prettyPrint = false);

    public TradesHistoryResponse ExecuteTradesHistoryCommand(ISyncApiConnector connector, long? start,
        long? end, bool prettyPrint = false);

    public TradesResponse ExecuteTradesCommand(ISyncApiConnector connector, bool openedOnly,
        bool prettyPrint = false);

    public SymbolResponse ExecuteSymbolCommand(ISyncApiConnector connector, string symbol,
        bool prettyPrint = false);

    public ChartLastResponse ExecuteChartLastCommand(ISyncApiConnector connector, string symbol,
        PERIOD_CODE period, long? start, bool prettyPrint = false);

    public ChartRangeResponse ExecuteChartRangeCommand(ISyncApiConnector connector, string symbol,
        PERIOD_CODE period, long? start, long? end, long? ticks, bool prettyPrint = false);

    public TickPricesResponse ExecuteTickPricesCommand(ISyncApiConnector connector, List<string> symbols,
        long? timestamp, bool prettyPrint = false);

    public TradeTransactionResponse ExecuteTradeTransactionCommand(ISyncApiConnector connector,
        TradeTransInfoRecord tradeTransInfo, bool prettyPrint = false);

    public AllSymbolsResponse ExecuteAllSymbolsCommand(ISyncApiConnector connector, bool prettyPrint = false);

    public TradingHoursResponse ExecuteTradingHoursCommand(ISyncApiConnector connector, List<string> symbols,
        bool prettyPrint = false);

    public CalendarResponse ExecuteCalendarCommand(ISyncApiConnector connector, bool prettyPrint = false);
}