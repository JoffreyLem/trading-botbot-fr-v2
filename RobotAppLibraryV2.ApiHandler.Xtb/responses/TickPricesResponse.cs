using Newtonsoft.Json.Linq;
using RobotAppLibraryV2.ApiHandler.Xtb.records;

namespace RobotAppLibraryV2.ApiHandler.Xtb.responses;

using JSONArray = JArray;
using JSONObject = JObject;

public class TickPricesResponse : BaseResponse
{
    private readonly LinkedList<TickRecord> ticks = new();

    public TickPricesResponse(string body) : base(body)
    {
        var ob = (JSONObject)ReturnData;
        var arr = (JSONArray)ob["quotations"];
        foreach (JSONObject e in arr)
        {
            var record = new TickRecord();
            record.FieldsFromJSONObject(e);
            ticks.AddLast(record);
        }
    }

    public virtual LinkedList<TickRecord> Ticks => ticks;
}