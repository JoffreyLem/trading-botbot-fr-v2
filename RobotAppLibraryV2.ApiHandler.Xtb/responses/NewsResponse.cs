using Newtonsoft.Json.Linq;
using RobotAppLibraryV2.ApiHandler.Xtb.records;

namespace RobotAppLibraryV2.ApiHandler.Xtb.responses;

using JSONArray = JArray;
using JSONObject = JObject;

public class NewsResponse : BaseResponse
{
    private readonly LinkedList<NewsTopicRecord> newsTopicRecords = new();

    public NewsResponse(string body) : base(body)
    {
        var arr = (JSONArray)ReturnData;
        foreach (JSONObject e in arr)
        {
            var record = new NewsTopicRecord();
            record.FieldsFromJSONObject(e);
            newsTopicRecords.AddLast(record);
        }
    }

    public virtual LinkedList<NewsTopicRecord> NewsTopicRecords => newsTopicRecords;
}