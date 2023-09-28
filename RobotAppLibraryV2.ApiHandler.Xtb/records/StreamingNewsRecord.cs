using Newtonsoft.Json.Linq;

namespace RobotAppLibraryV2.ApiHandler.Xtb.records;

using JSONObject = JObject;

public class StreamingNewsRecord : BaseResponseRecord
{
    public string Body { get; set; }

    public string Key { get; set; }

    public long? Time { get; set; }

    public string Title { get; set; }

    public void FieldsFromJSONObject(JSONObject value)
    {
        Body = (string)value["body"];
        Key = (string)value["key"];
        Time = (long?)value["time"];
        Title = (string)value["title"];
    }
}