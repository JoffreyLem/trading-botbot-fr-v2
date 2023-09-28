using Newtonsoft.Json.Linq;

namespace RobotAppLibraryV2.ApiHandler.Xtb.streaming;

using JSONObject = JObject;

internal class ProfitsSubscribe
{
    private readonly string streamSessionId;

    public ProfitsSubscribe(string streamSessionId)
    {
        this.streamSessionId = streamSessionId;
    }

    public override string ToString()
    {
        var result = new JSONObject();
        result.Add("command", "getProfits");
        result.Add("streamSessionId", streamSessionId);
        return result.ToString();
    }
}