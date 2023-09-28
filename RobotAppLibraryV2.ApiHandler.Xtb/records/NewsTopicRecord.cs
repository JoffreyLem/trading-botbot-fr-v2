using Newtonsoft.Json.Linq;

namespace RobotAppLibraryV2.ApiHandler.Xtb.records;

using JSONObject = JObject;

public class NewsTopicRecord : BaseResponseRecord
{
    private string body;
    private long? bodylen;
    private string key;
    private long? time;
    private string timeString;
    private string title;

    public virtual string Body => body;

    public virtual long? Bodylen => bodylen;

    [Obsolete("Field removed from API")] public virtual string Category => null;

    public virtual string Key => key;

    [Obsolete("Field removed from API")] public virtual LinkedList<string> Keywords => null;

    [Obsolete("Field removed from API")] public virtual long? Priority => null;

    [Obsolete("Field removed from API")] public virtual bool? Read => null;

    public virtual long? Time => time;

    public virtual string TimeString => timeString;

    public virtual string Title => title;

    [Obsolete("Use Title instead")] public virtual string Topic => title;

    public void FieldsFromJSONObject(JSONObject value)
    {
        body = (string)value["body"];
        bodylen = (long?)value["bodylen"];
        key = (string)value["key"];
        time = (long?)value["time"];
        timeString = (string)value["timeString"];
        title = (string)value["title"];
    }
}