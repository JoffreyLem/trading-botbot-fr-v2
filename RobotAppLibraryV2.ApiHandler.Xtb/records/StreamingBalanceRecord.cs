using Newtonsoft.Json.Linq;

namespace RobotAppLibraryV2.ApiHandler.Xtb.records;

using JSONObject = JObject;

public class StreamingBalanceRecord : BaseResponseRecord
{
    public double? Balance { get; set; }

    public double? Margin { get; set; }

    public double? MarginFree { get; set; }

    public double? MarginLevel { get; set; }

    public double? Equity { get; set; }

    public double? Credit { get; set; }

    public void FieldsFromJSONObject(JSONObject value)
    {
        Balance = (double?)value["balance"];
        Margin = (double?)value["margin"];
        MarginFree = (double?)value["marginFree"];
        MarginLevel = (double?)value["marginLevel"];
        Equity = (double?)value["equity"];
        Credit = (double?)value["credit"];
    }

    public override string ToString()
    {
        return "StreamingBalanceRecord{" +
               "balance=" + Balance +
               ", margin=" + Margin +
               ", marginFree=" + MarginFree +
               ", marginLevel=" + MarginLevel +
               ", equity=" + Equity +
               ", credit=" + Credit +
               '}';
    }
}