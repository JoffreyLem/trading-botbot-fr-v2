using Newtonsoft.Json.Linq;

namespace RobotAppLibraryV2.ApiHandler.Xtb.records;

using JSONObject = JObject;

public interface BaseResponseRecord
{
    void FieldsFromJSONObject(JSONObject value);
}