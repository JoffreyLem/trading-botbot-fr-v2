using Newtonsoft.Json.Linq;

namespace RobotAppLibraryV2.ApiHandler.Xtb.streaming;

using JSONObject = JObject;

internal class ProfitsStop
{
    public override string ToString()
    {
        var result = new JSONObject();
        result.Add("command", "stopProfits");
        return result.ToString();
    }
}