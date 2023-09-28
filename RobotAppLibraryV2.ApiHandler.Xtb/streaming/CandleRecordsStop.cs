using Newtonsoft.Json.Linq;

namespace RobotAppLibraryV2.ApiHandler.Xtb.streaming;

using JSONObject = JObject;

internal class CandleRecordsStop
{
    private readonly string symbol;

    public CandleRecordsStop(string symbol)
    {
        this.symbol = symbol;
    }

    public override string ToString()
    {
        var result = new JSONObject();
        result.Add("command", "stopCandles");
        result.Add("symbol", symbol);
        return result.ToString();
    }
}