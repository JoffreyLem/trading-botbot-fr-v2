using Newtonsoft.Json.Linq;
using RobotAppLibraryV2.ApiHandler.Xtb.codes;

namespace RobotAppLibraryV2.ApiHandler.Xtb.records;

using JSONObject = JObject;

public class TradeTransInfoRecord
{
    public TradeTransInfoRecord(TRADE_OPERATION_CODE cmd, TRADE_TRANSACTION_TYPE type, double? price, double? sl,
        double? tp, string symbol, double? volume, long? order, string customComment, long? expiration)
    {
        Cmd = cmd;
        Type = type;
        Price = price;
        Sl = sl;
        Tp = tp;
        Symbol = symbol;
        Volume = volume;
        Order = order;
        CustomComment = customComment;
        Expiration = expiration;
    }

    [Obsolete("Fields ie_devation and comment are not used anymore. Use another constructor instead.")]
    public TradeTransInfoRecord(TRADE_OPERATION_CODE cmd, TRADE_TRANSACTION_TYPE type, double? price, double? sl,
        double? tp, string symbol, double? volume, long? ie_deviation, long? order, string comment, long? expiration)
    {
        Cmd = cmd;
        Type = type;
        Price = price;
        Sl = sl;
        Tp = tp;
        Symbol = symbol;
        Volume = volume;
        Order = order;
        Expiration = expiration;
        CustomComment = comment;
    }

    public TRADE_OPERATION_CODE Cmd { get; set; }

    public string CustomComment { get; set; }

    public long? Expiration { get; set; }

    public long? Order { get; set; }

    public double? Price { get; set; }

    public double? Sl { get; set; }

    public string Symbol { get; set; }

    public double? Tp { get; set; }

    public TRADE_TRANSACTION_TYPE Type { get; set; }

    public double? Volume { get; set; }

    public virtual JSONObject toJSONObject()
    {
        var obj = new JSONObject();
        obj.Add("cmd", Cmd.Code);
        obj.Add("type", Type.Code);
        obj.Add("price", Price);
        obj.Add("sl", Sl);
        obj.Add("tp", Tp);
        obj.Add("symbol", Symbol);
        obj.Add("volume", Volume);
        obj.Add("order", Order);
        obj.Add("customComment", CustomComment);
        obj.Add("expiration", Expiration);
        return obj;
    }

    public override string ToString()
    {
        return "TradeTransInfo [" +
               Cmd + ", " +
               Type + ", " +
               Price + ", " +
               Sl + ", " +
               Tp + ", " +
               Symbol + ", " +
               Volume +
               Order + ", " +
               CustomComment + ", " +
               Expiration + ", " +
               "]";
    }
}