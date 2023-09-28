using Newtonsoft.Json.Linq;

namespace RobotAppLibraryV2.ApiHandler.Xtb.streaming;

using JSONObject = JObject;

internal class TickPricesSubscribe
{
    private readonly long? maxLevel;
    private readonly long? minArrivalTime;
    private readonly string streamSessionId;
    private readonly string symbol;

    public TickPricesSubscribe(string symbol, string streamSessionId, long? minArrivalTime = null,
        long? maxLevel = null)
    {
        this.symbol = symbol;
        this.minArrivalTime = minArrivalTime;
        this.streamSessionId = streamSessionId;
        this.maxLevel = maxLevel;
    }

    public override string ToString()
    {
        var result = new JSONObject();
        result.Add("command", "getTickPrices");
        result.Add("symbol", symbol);

        if (minArrivalTime.HasValue) result.Add("minArrivalTime", minArrivalTime);

        if (maxLevel.HasValue) result.Add("maxLevel", maxLevel);

        result.Add("streamSessionId", streamSessionId);
        return result.ToString();
    }
}