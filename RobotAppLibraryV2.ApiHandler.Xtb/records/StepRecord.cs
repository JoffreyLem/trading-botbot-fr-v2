using Newtonsoft.Json.Linq;

namespace RobotAppLibraryV2.ApiHandler.Xtb.records;

using JSONObject = JObject;

public class StepRecord : BaseResponseRecord
{
    private double FromValue;
    private double Step;

    public void FieldsFromJSONObject(JSONObject value)
    {
        FromValue = (double)value["fromValue"];
        Step = (double)value["step"];
    }
}