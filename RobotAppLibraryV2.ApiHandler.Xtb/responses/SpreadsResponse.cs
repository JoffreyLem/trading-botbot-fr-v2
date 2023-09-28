using Newtonsoft.Json.Linq;
using RobotAppLibraryV2.ApiHandler.Xtb.records;

namespace RobotAppLibraryV2.ApiHandler.Xtb.responses;

using JSONArray = JArray;
using JSONObject = JObject;

public class SpreadsResponse : BaseResponse
{
    private readonly LinkedList<SpreadRecord> spreadRecords = new();

    public SpreadsResponse(string body)
        : base(body)
    {
        var symbolRecords = (JSONArray)ReturnData;
        foreach (JSONObject e in symbolRecords)
        {
            var spreadRecord = new SpreadRecord();
            spreadRecord.FieldsFromJSONObject(e);
            spreadRecords.AddLast(spreadRecord);
        }
    }

    public virtual LinkedList<SpreadRecord> SpreadRecords => spreadRecords;
}