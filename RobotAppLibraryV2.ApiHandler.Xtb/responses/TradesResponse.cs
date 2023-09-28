using Newtonsoft.Json.Linq;
using RobotAppLibraryV2.ApiHandler.Xtb.records;

namespace RobotAppLibraryV2.ApiHandler.Xtb.responses;

using JSONArray = JArray;
using JSONObject = JObject;

public class TradesResponse : BaseResponse
{
    private readonly LinkedList<TradeRecord> tradeRecords = new();

    public TradesResponse(string body) : base(body)
    {
        var arr = (JSONArray)ReturnData;
        foreach (JSONObject e in arr)
        {
            var record = new TradeRecord();
            record.FieldsFromJSONObject(e);
            tradeRecords.AddLast(record);
        }
    }

    public virtual LinkedList<TradeRecord> TradeRecords => tradeRecords;
}