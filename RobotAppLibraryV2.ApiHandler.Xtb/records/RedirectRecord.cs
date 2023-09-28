using JSONObject = Newtonsoft.Json.Linq.JObject;

namespace RobotAppLibraryV2.ApiHandler.Xtb.records;

public class RedirectRecord : BaseResponseRecord
{
    public int MainPort { get; private set; }

    public int StreamingPort { get; private set; }

    public string Address { get; private set; }

    public void FieldsFromJSONObject(JSONObject value)
    {
        MainPort = (int)value["mainPort"];
        StreamingPort = (int)value["streamingPort"];
        Address = (string)value["address"];
    }

    public override string ToString()
    {
        return "RedirectRecord [" +
               "mainPort=" + MainPort +
               ", streamingPort=" + StreamingPort +
               ", address=" + Address + "]";
    }
}