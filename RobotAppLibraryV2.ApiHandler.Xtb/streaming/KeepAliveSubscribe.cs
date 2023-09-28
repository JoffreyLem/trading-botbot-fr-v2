using Newtonsoft.Json.Linq;

namespace RobotAppLibraryV2.ApiHandler.Xtb.streaming;

using JSONObject = JObject;

internal class KeepAliveSubscribe
{
    private readonly string streamSessionId;

    public KeepAliveSubscribe(string streamSessionId)
    {
        this.streamSessionId = streamSessionId;
    }

    public override string ToString()
    {
        var result = new JSONObject();
        result.Add("command", "getKeepAlive");
        result.Add("streamSessionId", streamSessionId);
        return result.ToString();
    }
}