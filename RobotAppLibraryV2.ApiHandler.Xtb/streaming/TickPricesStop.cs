using Newtonsoft.Json.Linq;

namespace RobotAppLibraryV2.ApiHandler.Xtb.streaming;

using JSONObject = JObject;

internal class TickPricesStop
{
    private readonly string symbol;

    public TickPricesStop(string symbol)
    {
        this.symbol = symbol;
    }

    public override string ToString()
    {
        var result = new JSONObject();
        result.Add("command", "stopTickPrices");
        result.Add("symbol", symbol);
        return result.ToString();
    }
}