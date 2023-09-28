using Newtonsoft.Json.Linq;
using RobotAppLibraryV2.ApiHandler.Xtb.records;

namespace RobotAppLibraryV2.ApiHandler.Xtb.responses;

using JSONArray = JArray;
using JSONObject = JObject;

public class TradingHoursResponse : BaseResponse
{
    private readonly LinkedList<TradingHoursRecord> tradingHoursRecords = new();

    public TradingHoursResponse(string body) : base(body)
    {
        var ob = (JSONArray)ReturnData;
        foreach (JSONObject e in ob)
        {
            var record = new TradingHoursRecord();
            record.FieldsFromJSONObject(e);
            tradingHoursRecords.AddLast(record);
        }
    }

    public virtual LinkedList<TradingHoursRecord> TradingHoursRecords => tradingHoursRecords;
}