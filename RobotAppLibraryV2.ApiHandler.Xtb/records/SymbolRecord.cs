using Newtonsoft.Json.Linq;
using RobotAppLibraryV2.ApiHandler.Xtb.codes;

namespace RobotAppLibraryV2.ApiHandler.Xtb.records;

using JSONObject = JObject;

public class SymbolRecord : BaseResponseRecord
{
    private double? spreadRaw;
    private double? spreadTable;

    public virtual double? Ask { get; set; }

    public virtual double? Bid { get; set; }

    public virtual string CategoryName { get; set; }

    public virtual long? ContractSize { get; set; }

    public virtual string Currency { get; set; }

    public virtual bool? CurrencyPair { get; set; }

    [Obsolete("Use Precision instead")] public virtual long? Digits => Precision;

    public string CurrencyProfit { get; set; }

    public virtual string Description { get; set; }

    public virtual long? Expiration { get; set; }

    public virtual string GroupName { get; set; }

    public virtual double? High { get; set; }

    public virtual long? InitialMargin { get; set; }

    public virtual long? InstantMaxVolume { get; set; }

    public virtual double? Leverage { get; set; }

    public virtual bool? LongOnly { get; set; }

    public virtual double? LotMax { get; set; }

    public virtual double? LotMin { get; set; }

    public virtual double? LotStep { get; set; }

    public virtual double? Low { get; set; }

    public virtual long? MarginHedged { get; set; }

    public virtual bool? MarginHedgedStrong { get; set; }

    public virtual long? MarginMaintenance { get; set; }

    public virtual MARGIN_MODE MarginMode { get; set; }

    public virtual long? Precision { get; set; }

    public virtual double? Percentage { get; set; }

    public virtual PROFIT_MODE ProfitMode { get; set; }

    public long? QuoteId { get; set; }

    public virtual double? SpreadRaw
    {
        get => spreadRaw;
        set => spreadTable = value;
    }

    public virtual double? SpreadTable
    {
        get => spreadTable;
        set => spreadTable = value;
    }

    public virtual long? Starting { get; set; }

    public virtual long? StepRuleId { get; set; }

    public virtual long? StopsLevel { get; set; }

    public virtual bool? SwapEnable { get; set; }

    public virtual double? SwapLong { get; set; }

    public virtual double? SwapShort { get; set; }

    public virtual SWAP_TYPE SwapType { get; set; }

    public virtual SWAP_ROLLOVER_TYPE SwapRollover { get; set; }

    public virtual string Symbol { get; set; }

    public virtual double? TickSize { get; set; }

    public virtual double? TickValue { get; set; }

    public virtual long? Time { get; set; }

    public virtual string TimeString { get; set; }

    public virtual long? Type { get; set; }

    public void FieldsFromJSONObject(JSONObject value)
    {
        Ask = (double?)value["ask"];
        Bid = (double?)value["bid"];
        CategoryName = (string)value["categoryName"];
        Currency = (string)value["currency"];
        CurrencyPair = (bool?)value["currencyPair"];
        CurrencyProfit = (string)value["currencyProfit"];
        Description = (string)value["description"];
        Expiration = (long?)value["expiration"];
        GroupName = (string)value["groupName"];
        High = (double?)value["high"];
        InstantMaxVolume = (long?)value["instantMaxVolume"];
        Leverage = (double)value["leverage"];
        LongOnly = (bool?)value["longOnly"];
        LotMax = (double?)value["lotMax"];
        LotMin = (double?)value["lotMin"];
        LotStep = (double?)value["lotStep"];
        Low = (double?)value["low"];
        Precision = (long?)value["precision"];
        Starting = (long?)value["starting"];
        StopsLevel = (long?)value["stopsLevel"];
        Symbol = (string)value["symbol"];
        Time = (long?)value["time"];
        TimeString = (string)value["timeString"];
        Type = (long?)value["type"];
        ContractSize = (long?)value["contractSize"];
        InitialMargin = (long?)value["initialMargin"];
        MarginHedged = (long?)value["marginHedged"];
        MarginHedgedStrong = (bool?)value["marginHedgedStrong"];
        MarginMaintenance = (long?)value["marginMaintenance"];
        MarginMode = new MARGIN_MODE((long)value["marginMode"]);
        Percentage = (double?)value["percentage"];
        ProfitMode = new PROFIT_MODE((long)value["profitMode"]);
        QuoteId = (long?)value["quoteId"];
        SpreadRaw = (double?)value["spreadRaw"];
        SpreadTable = (double?)value["spreadTable"];
        StepRuleId = (long?)value["stepRuleId"];
        SwapEnable = (bool?)value["swapEnable"];
        SwapLong = (double?)value["swapLong"];
        SwapShort = (double?)value["swapShort"];
        SwapType = new SWAP_TYPE((long)value["swapType"]);
        SwapRollover = new SWAP_ROLLOVER_TYPE((long)value["swap_rollover3days"]);
        TickSize = (double?)value["tickSize"];
        TickValue = (double?)value["tickValue"];
    }
}