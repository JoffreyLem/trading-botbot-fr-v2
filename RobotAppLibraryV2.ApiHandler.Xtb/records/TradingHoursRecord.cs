using Newtonsoft.Json.Linq;

namespace RobotAppLibraryV2.ApiHandler.Xtb.records;

using JSONObject = JObject;
using JSONArray = JArray;

public class TradingHoursRecord : BaseResponseRecord
{
    private LinkedList<HoursRecord> quotes = new();
    private string symbol;
    private LinkedList<HoursRecord> trading = new();

    public virtual string Symbol => symbol;

    public virtual LinkedList<HoursRecord> Quotes => quotes;

    public virtual LinkedList<HoursRecord> Trading => trading;

    public void FieldsFromJSONObject(JSONObject value)
    {
        FieldsFromJSONObject(value, null);
    }

    public bool FieldsFromJSONObject(JSONObject value, string str)
    {
        symbol = (string)value["symbol"];
        quotes = new LinkedList<HoursRecord>();
        if (value["quotes"] != null)
        {
            var jsonarray = (JSONArray)value["quotes"];
            foreach (JSONObject i in jsonarray)
            {
                var rec = new HoursRecord();
                rec.FieldsFromJSONObject(i);
                quotes.AddLast(rec);
            }
        }

        trading = new LinkedList<HoursRecord>();
        if (value["trading"] != null)
        {
            var jsonarray = (JSONArray)value["trading"];
            foreach (JSONObject i in jsonarray)
            {
                var rec = new HoursRecord();
                rec.FieldsFromJSONObject(i);
                trading.AddLast(rec);
            }
        }

        if (symbol == null || quotes.Count == 0 || trading.Count == 0) return false;

        return true;
    }

    public override string ToString()
    {
        return "TradingHoursRecord{" + "symbol=" + symbol + ", quotes=" + quotes + ", trading=" + trading + '}';
    }
}