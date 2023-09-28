using Newtonsoft.Json.Linq;

namespace RobotAppLibraryV2.ApiHandler.Xtb.records;

using JSONObject = JObject;

public class StreamingTickRecord : BaseResponseRecord
{
    public double? Ask { get; set; }

    public double? Bid { get; set; }

    public long? AskVolume { get; set; }

    public long? BidVolume { get; set; }

    public double? High { get; set; }

    public double? Low { get; set; }

    public string Symbol { get; set; }

    public double? SpreadRaw { get; set; }

    public double? SpreadTable { get; set; }

    public long? Timestamp { get; set; }

    public long? Level { get; set; }

    public long? QuoteId { get; set; }

    public void FieldsFromJSONObject(JSONObject value)
    {
        Ask = (double?)value["ask"];
        Bid = (double?)value["bid"];
        AskVolume = (long?)value["askVolume"];
        BidVolume = (long?)value["bidVolume"];
        High = (double?)value["high"];
        Low = (double?)value["low"];
        Symbol = (string)value["symbol"];
        Timestamp = (long?)value["timestamp"];
        Level = (long?)value["level"];
        QuoteId = (long?)value["quoteId"];
        SpreadRaw = (double?)value["spreadRaw"];
        SpreadTable = (double?)value["spreadTable"];
    }

    public override string ToString()
    {
        return "StreamingTickRecord{" +
               "ask=" + Ask +
               ", bid=" + Bid +
               ", askVolume=" + AskVolume +
               ", bidVolume=" + BidVolume +
               ", high=" + High +
               ", low=" + Low +
               ", symbol=" + Symbol +
               ", timestamp=" + Timestamp +
               ", level=" + Level +
               ", quoteId=" + QuoteId +
               ", spreadRaw=" + SpreadRaw +
               ", spreadTable=" + SpreadTable +
               '}';
    }
}