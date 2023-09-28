using Newtonsoft.Json.Linq;

namespace RobotAppLibraryV2.ApiHandler.Xtb.streaming;

using JSONObject = JObject;

internal class TradeStatusRecordsSubscribe
{
    private readonly string streamSessionId;

    public TradeStatusRecordsSubscribe(string streamSessionId)
    {
        this.streamSessionId = streamSessionId;
    }

    public override string ToString()
    {
        var result = new JSONObject();
        result.Add("command", "getTradeStatus");
        result.Add("streamSessionId", streamSessionId);
        return result.ToString();
    }
}