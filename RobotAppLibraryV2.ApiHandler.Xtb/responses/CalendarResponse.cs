using Newtonsoft.Json.Linq;
using RobotAppLibraryV2.ApiHandler.Xtb.records;

namespace RobotAppLibraryV2.ApiHandler.Xtb.responses;

using JSONArray = JArray;
using JSONObject = JObject;

public class CalendarResponse : BaseResponse
{
    public CalendarResponse(string body)
        : base(body)
    {
        var returnData = (JSONArray)ReturnData;

        foreach (JSONObject e in returnData)
        {
            var record = new CalendarRecord();
            record.FieldsFromJSONObject(e);
            CalendarRecords.Add(record);
        }
    }

    public List<CalendarRecord> CalendarRecords { get; } = new();
}