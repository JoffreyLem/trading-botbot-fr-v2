using Newtonsoft.Json.Linq;
using RobotAppLibraryV2.ApiHandler.Xtb.codes;
using RobotAppLibraryV2.ApiHandler.Xtb.commands;
using RobotAppLibraryV2.ApiHandler.Xtb.errors;
using RobotAppLibraryV2.ApiHandler.Xtb.records;
using RobotAppLibraryV2.ApiHandler.Xtb.responses;
using RobotAppLibraryV2.ApiHandler.Xtb.sync;

namespace RobotAppLibraryV2.ApiHandler.Xtb;

using JSONArray = JArray;
using JSONObject = JObject;

public class APICommandFactory : IApiCommandFactory
{
    /// <summary>
    ///     Counts redirections.
    /// </summary>
    private static int redirectCounter;

    #region Command creators

    public LoginCommand CreateLoginCommand(string userId, string password, bool prettyPrint = false)
    {
        var args = new JSONObject();
        args.Add("userId", userId);
        args.Add("password", password);
        args.Add("type", "dotNET");
        args.Add("version", SyncAPIConnector.VERSION);
        return new LoginCommand(args, prettyPrint);
    }

    [Obsolete("Up from 2.3.3 login is not a long, but string")]
    public LoginCommand CreateLoginCommand(long? userId, string password, bool prettyPrint = false)
    {
        return CreateLoginCommand(userId.Value.ToString(), password, prettyPrint);
    }

    public LoginCommand CreateLoginCommand(Credentials credentials, bool prettyPrint = false)
    {
        var jsonObj = CreateLoginJsonObject(credentials);
        return new LoginCommand(jsonObj, prettyPrint);
    }

    private JSONObject CreateLoginJsonObject(Credentials credentials)
    {
        var response = new JSONObject();
        if (credentials != null)
        {
            response.Add("userId", credentials.Login);
            response.Add("password", credentials.Password);
            response.Add("type", "dotNET");
            response.Add("version", SyncAPIConnector.VERSION);

            if (credentials.AppId != null) response.Add("appId", credentials.AppId);

            if (credentials.AppName != null) response.Add("appName", credentials.AppName);
        }

        return response;
    }

    public AllSymbolsCommand CreateAllSymbolsCommand(bool prettyPrint = false)
    {
        return new AllSymbolsCommand(prettyPrint);
    }

    public CalendarCommand CreateCalendarCommand(bool prettyPrint = false)
    {
        return new CalendarCommand(prettyPrint);
    }

    public ChartLastCommand CreateChartLastCommand(string symbol, PERIOD_CODE period, long? start,
        bool prettyPrint = false)
    {
        var args = new JSONObject();
        args.Add("info", new ChartLastInfoRecord(symbol, period, start).toJSONObject());
        return new ChartLastCommand(args, prettyPrint);
    }

    public ChartLastCommand CreateChartLastCommand(ChartLastInfoRecord info, bool prettyPrint = false)
    {
        var args = new JSONObject();
        args.Add("info", info.toJSONObject());
        return new ChartLastCommand(args, prettyPrint);
    }

    public ChartRangeCommand CreateChartRangeCommand(ChartRangeInfoRecord info, bool prettyPrint = false)
    {
        var args = new JSONObject();
        args.Add("info", info.toJSONObject());
        return new ChartRangeCommand(args, prettyPrint);
    }

    public ChartRangeCommand CreateChartRangeCommand(string symbol, PERIOD_CODE period, long? start, long? end,
        long? ticks, bool prettyPrint = false)
    {
        var args = new JSONObject();
        args.Add("info", new ChartRangeInfoRecord(symbol, period, start, end, ticks).toJSONObject());
        return new ChartRangeCommand(args, prettyPrint);
    }

    public CommissionDefCommand CreateCommissionDefCommand(string symbol, double? volume, bool prettyPrint = false)
    {
        var args = new JSONObject();
        args.Add("symbol", symbol);
        args.Add("volume", volume);
        return new CommissionDefCommand(args, prettyPrint);
    }

    public LogoutCommand CreateLogoutCommand()
    {
        return new LogoutCommand();
    }

    public MarginLevelCommand CreateMarginLevelCommand(bool prettyPrint = false)
    {
        return new MarginLevelCommand(prettyPrint);
    }

    public MarginTradeCommand CreateMarginTradeCommand(string symbol, double? volume, bool prettyPrint)
    {
        var args = new JSONObject();
        args.Add("symbol", symbol);
        args.Add("volume", volume);
        return new MarginTradeCommand(args, prettyPrint);
    }

    public NewsCommand CreateNewsCommand(long? start, long? end, bool prettyPrint)
    {
        var args = new JSONObject();
        args.Add("start", start);
        args.Add("end", end);
        return new NewsCommand(args, prettyPrint);
    }

    public ServerTimeCommand CreateServerTimeCommand(bool prettyPrint = false)
    {
        return new ServerTimeCommand(prettyPrint);
    }

    public CurrentUserDataCommand CreateCurrentUserDataCommand(bool prettyPrint = false)
    {
        return new CurrentUserDataCommand(prettyPrint);
    }

    public IbsHistoryCommand CreateGetIbsHistoryCommand(long start, long end, bool prettyPrint = false)
    {
        var args = new JSONObject();
        args.Add("start", start);
        args.Add("end", end);
        return new IbsHistoryCommand(args, prettyPrint);
    }

    public PingCommand CreatePingCommand(bool prettyPrint = false)
    {
        return new PingCommand(prettyPrint);
    }

    public ProfitCalculationCommand CreateProfitCalculationCommand(string symbol, double? volume,
        TRADE_OPERATION_CODE cmd, double? openPrice, double? closePrice, bool prettyPrint = false)
    {
        var args = new JSONObject();
        args.Add("symbol", symbol);
        args.Add("volume", volume);
        args.Add("cmd", cmd.Code);
        args.Add("openPrice", openPrice);
        args.Add("closePrice", closePrice);
        return new ProfitCalculationCommand(args, prettyPrint);
    }

    [Obsolete("Command not available in API any more")]
    public AllSymbolGroupsCommand CreateSymbolGroupsCommand(bool prettyPrint = false)
    {
        return null;
    }

    public SymbolCommand CreateSymbolCommand(string symbol, bool prettyPrint = false)
    {
        var args = new JSONObject();
        args.Add("symbol", symbol);
        return new SymbolCommand(args, prettyPrint);
    }

    public StepRulesCommand CreateStepRulesCommand(bool prettyPrint = false)
    {
        return new StepRulesCommand();
    }

    public TickPricesCommand CreateTickPricesCommand(List<string> symbols, long? timestamp, bool prettyPrint = false)
    {
        var args = new JSONObject();
        var arr = new JSONArray();
        foreach (var symbol in symbols) arr.Add(symbol);

        args.Add("symbols", arr);
        args.Add("timestamp", timestamp);
        args.Add("level", 0);
        return new TickPricesCommand(args, prettyPrint);
    }

    public TradeRecordsCommand CreateTradeRecordsCommand(LinkedList<long?> orders, bool prettyPrint = false)
    {
        var args = new JSONObject();
        var arr = new JSONArray();
        foreach (var order in orders) arr.Add(order);

        args.Add("orders", arr);
        return new TradeRecordsCommand(args, prettyPrint);
    }

    public TradeTransactionCommand CreateTradeTransactionCommand(TradeTransInfoRecord tradeTransInfo,
        bool prettyPrint = false)
    {
        var args = new JSONObject();
        args.Add("tradeTransInfo", tradeTransInfo.toJSONObject());
        return new TradeTransactionCommand(args, prettyPrint);
    }

    public TradeTransactionCommand CreateTradeTransactionCommand(TRADE_OPERATION_CODE cmd, TRADE_TRANSACTION_TYPE type,
        double? price, double? sl, double? tp, string symbol, double? volume, long? order, string customComment,
        long? expiration, bool prettyPrint = false)
    {
        var args = new JSONObject();
        args.Add("tradeTransInfo",
            new TradeTransInfoRecord(cmd, type, price, sl, tp, symbol, volume, order, customComment, expiration)
                .toJSONObject());
        return new TradeTransactionCommand(args, prettyPrint);
    }

    [Obsolete("Method outdated. ie_deviation and comment are not available any more")]
    public TradeTransactionCommand CreateTradeTransactionCommand(TRADE_OPERATION_CODE cmd, TRADE_TRANSACTION_TYPE type,
        double? price, double? sl, double? tp, string symbol, double? volume, long? ie_deviation, long? order,
        string comment, long? expiration, bool prettyPrint = false)
    {
        return CreateTradeTransactionCommand(cmd, type, price, sl, tp, symbol, volume, order, "", expiration);
    }

    public TradeTransactionStatusCommand CreateTradeTransactionStatusCommand(long? order, bool prettyPrint = false)
    {
        var args = new JSONObject();
        args.Add("order", order);
        return new TradeTransactionStatusCommand(args, prettyPrint);
    }

    public TradesCommand CreateTradesCommand(bool openedOnly, bool prettyPrint = false)
    {
        var args = new JSONObject();
        args.Add("openedOnly", openedOnly);
        return new TradesCommand(args, prettyPrint);
    }

    public TradesHistoryCommand CreateTradesHistoryCommand(long? start, long? end, bool prettyPrint = false)
    {
        var args = new JSONObject();
        args.Add("start", start);
        args.Add("end", end);
        return new TradesHistoryCommand(args, prettyPrint);
    }

    public TradingHoursCommand CreateTradingHoursCommand(List<string> symbols, bool prettyPrint = false)
    {
        var args = new JSONObject();
        var arr = new JSONArray();
        foreach (var symbol in symbols) arr.Add(symbol);

        args.Add("symbols", arr);
        return new TradingHoursCommand(args, prettyPrint);
    }

    public VersionCommand CreateVersionCommand(bool prettyPrint = false)
    {
        var args = new JSONObject();
        return new VersionCommand(args, prettyPrint);
    }

    #endregion

    #region Command executors

    public AllSymbolsResponse ExecuteAllSymbolsCommand(ISyncApiConnector connector, bool prettyPrint = false)
    {
        return new AllSymbolsResponse(connector.ExecuteCommand(CreateAllSymbolsCommand(prettyPrint)));
    }

    public CalendarResponse ExecuteCalendarCommand(ISyncApiConnector connector, bool prettyPrint = false)
    {
        return new CalendarResponse(connector.ExecuteCommand(CreateCalendarCommand(prettyPrint)).ToString());
    }

    public ChartLastResponse ExecuteChartLastCommand(ISyncApiConnector connector, ChartLastInfoRecord info,
        bool prettyPrint = false)
    {
        return new ChartLastResponse(connector.ExecuteCommand(CreateChartLastCommand(info, prettyPrint)).ToString());
    }

    public ChartLastResponse ExecuteChartLastCommand(ISyncApiConnector connector, string symbol, PERIOD_CODE period,
        long? start, bool prettyPrint = false)
    {
        return new ChartLastResponse(connector
            .ExecuteCommand(CreateChartLastCommand(symbol, period, start, prettyPrint)).ToString());
    }

    public ChartRangeResponse ExecuteChartRangeCommand(ISyncApiConnector connector, ChartRangeInfoRecord info,
        bool prettyPrint = false)
    {
        return new ChartRangeResponse(connector.ExecuteCommand(CreateChartRangeCommand(info, prettyPrint)).ToString());
    }

    public ChartRangeResponse ExecuteChartRangeCommand(ISyncApiConnector connector, string symbol, PERIOD_CODE period,
        long? start, long? end, long? ticks, bool prettyPrint = false)
    {
        return new ChartRangeResponse(connector
            .ExecuteCommand(CreateChartRangeCommand(symbol, period, start, end, ticks, prettyPrint)).ToString());
    }

    public CommissionDefResponse ExecuteCommissionDefCommand(ISyncApiConnector connector, string symbol, double? volume,
        bool prettyPrint = false)
    {
        return new CommissionDefResponse(connector
            .ExecuteCommand(CreateCommissionDefCommand(symbol, volume, prettyPrint)).ToString());
    }

    public IbsHistoryResponse ExecuteIbsHistoryCommand(ISyncApiConnector connector, long start, long end,
        bool prettyPrint = false)
    {
        return new IbsHistoryResponse(connector.ExecuteCommand(CreateGetIbsHistoryCommand(start, end, prettyPrint))
            .ToString());
    }

    [Obsolete("Up from 2.3.3 login is not a long, but string")]
    public LoginResponse ExecuteLoginCommand(ISyncApiConnector connector, long userId, string password,
        bool prettyPrint = false)
    {
        return ExecuteLoginCommand(connector, userId.ToString(), password, prettyPrint);
    }

    public LoginResponse ExecuteLoginCommand(ISyncApiConnector connector, string userId, string password,
        bool prettyPrint = false)
    {
        var credentials = new Credentials(userId, password);
        return ExecuteLoginCommand(connector, credentials, prettyPrint);
    }

    public LoginResponse ExecuteLoginCommand(ISyncApiConnector connector, Credentials credentials,
        bool prettyPrint = false)
    {
        var loginCommand = CreateLoginCommand(credentials, prettyPrint);
        var loginResponse = new LoginResponse(connector.ExecuteCommand(loginCommand).ToString());

        redirectCounter = 0;

        while (loginResponse.RedirectRecord != null)
        {
            if (redirectCounter >= SyncAPIConnector.MAX_REDIRECTS)
                throw new APICommunicationException("too many redirects");

            var newServer = new Server(loginResponse.RedirectRecord.Address, loginResponse.RedirectRecord.MainPort,
                loginResponse.RedirectRecord.StreamingPort, true,
                "Redirected to: " + loginResponse.RedirectRecord.Address + ":" + loginResponse.RedirectRecord.MainPort +
                "/" + loginResponse.RedirectRecord.StreamingPort);
            connector.Redirect(newServer);
            redirectCounter++;
            loginResponse = new LoginResponse(connector.ExecuteCommand(loginCommand).ToString());
        }

        if (loginResponse.StreamSessionId != null)
            connector.StreamingApiConnector.StreamSessionId = loginResponse.StreamSessionId;

        return loginResponse;
    }

    public LogoutResponse ExecuteLogoutCommand(ISyncApiConnector connector)
    {
        return new LogoutResponse(connector.ExecuteCommand(CreateLogoutCommand()).ToString());
    }

    public MarginLevelResponse ExecuteMarginLevelCommand(ISyncApiConnector connector, bool prettyPrint = false)
    {
        return new MarginLevelResponse(connector.ExecuteCommand(CreateMarginLevelCommand(prettyPrint)).ToString());
    }

    public MarginTradeResponse ExecuteMarginTradeCommand(ISyncApiConnector connector, string symbol, double? volume,
        bool prettyPrint)
    {
        return new MarginTradeResponse(connector.ExecuteCommand(CreateMarginTradeCommand(symbol, volume, prettyPrint))
            .ToString());
    }

    public NewsResponse ExecuteNewsCommand(ISyncApiConnector connector, long? start, long? end,
        bool prettyPrint = false)
    {
        return new NewsResponse(connector.ExecuteCommand(CreateNewsCommand(start, end, prettyPrint)).ToString());
    }

    public ServerTimeResponse ExecuteServerTimeCommand(ISyncApiConnector connector, bool prettyPrint = false)
    {
        return new ServerTimeResponse(connector.ExecuteCommand(CreateServerTimeCommand(prettyPrint)).ToString());
    }

    public CurrentUserDataResponse ExecuteCurrentUserDataCommand(ISyncApiConnector connector, bool prettyPrint = false)
    {
        return new CurrentUserDataResponse(connector.ExecuteCommand(CreateCurrentUserDataCommand(prettyPrint))
            .ToString());
    }

    public PingResponse ExecutePingCommand(ISyncApiConnector connector, bool prettyPrint = false)
    {
        return new PingResponse(connector.ExecuteCommand(CreatePingCommand(prettyPrint)).ToString());
    }

    public ProfitCalculationResponse ExecuteProfitCalculationCommand(ISyncApiConnector connector, string symbol,
        double? volume, TRADE_OPERATION_CODE cmd, double? openPrice, double? closePrice, bool prettyPrint = false)
    {
        return new ProfitCalculationResponse(connector
            .ExecuteCommand(CreateProfitCalculationCommand(symbol, volume, cmd, openPrice, closePrice, prettyPrint))
            .ToString());
    }

    public StepRulesResponse ExecuteStepRulesCommand(ISyncApiConnector connector, bool prettyPrint = false)
    {
        return new StepRulesResponse(connector.ExecuteCommand(CreateStepRulesCommand(prettyPrint)).ToString());
    }

    public SymbolResponse ExecuteSymbolCommand(ISyncApiConnector connector, string symbol, bool prettyPrint = false)
    {
        return new SymbolResponse(connector.ExecuteCommand(CreateSymbolCommand(symbol, prettyPrint)).ToString());
    }

    public TickPricesResponse ExecuteTickPricesCommand(ISyncApiConnector connector, List<string> symbols,
        long? timestamp, bool prettyPrint = false)
    {
        return new TickPricesResponse(connector.ExecuteCommand(CreateTickPricesCommand(symbols, timestamp, prettyPrint))
            .ToString());
    }

    public TradeRecordsResponse ExecuteTradeRecordsCommand(ISyncApiConnector connector, LinkedList<long?> orders,
        bool prettyPrint = false)
    {
        return new TradeRecordsResponse(connector.ExecuteCommand(CreateTradeRecordsCommand(orders, prettyPrint))
            .ToString());
    }

    public TradeTransactionResponse ExecuteTradeTransactionCommand(ISyncApiConnector connector,
        TradeTransInfoRecord tradeTransInfo, bool prettyPrint = false)
    {
        return new TradeTransactionResponse(connector
            .ExecuteCommand(CreateTradeTransactionCommand(tradeTransInfo, prettyPrint)).ToString());
    }

    public TradeTransactionResponse ExecuteTradeTransactionCommand(ISyncApiConnector connector,
        TRADE_OPERATION_CODE cmd, TRADE_TRANSACTION_TYPE type, double? price, double? sl, double? tp, string symbol,
        double? volume, long? order, string customComment, long? expiration, bool prettyPrint = false)
    {
        return new TradeTransactionResponse(connector.ExecuteCommand(CreateTradeTransactionCommand(cmd, type, price, sl,
            tp, symbol, volume, order, customComment, expiration, prettyPrint)).ToString());
    }


    public TradeTransactionStatusResponse ExecuteTradeTransactionStatusCommand(ISyncApiConnector connector, long? order,
        bool prettyPrint = false)
    {
        return new TradeTransactionStatusResponse(connector
            .ExecuteCommand(CreateTradeTransactionStatusCommand(order, prettyPrint)).ToString());
    }

    public TradesResponse ExecuteTradesCommand(ISyncApiConnector connector, bool openedOnly, bool prettyPrint = false)
    {
        return new TradesResponse(connector.ExecuteCommand(CreateTradesCommand(openedOnly, prettyPrint)).ToString());
    }

    public TradesHistoryResponse ExecuteTradesHistoryCommand(ISyncApiConnector connector, long? start, long? end,
        bool prettyPrint = false)
    {
        return new TradesHistoryResponse(connector.ExecuteCommand(CreateTradesHistoryCommand(start, end, prettyPrint))
            .ToString());
    }

    public TradingHoursResponse ExecuteTradingHoursCommand(ISyncApiConnector connector, List<string> symbols,
        bool prettyPrint = false)
    {
        return new TradingHoursResponse(connector.ExecuteCommand(CreateTradingHoursCommand(symbols, prettyPrint))
            .ToString());
    }

    public VersionResponse ExecuteVersionCommand(ISyncApiConnector connector, bool prettyPrint = false)
    {
        return new VersionResponse(connector.ExecuteCommand(CreateVersionCommand(prettyPrint)).ToString());
    }

    #endregion
}