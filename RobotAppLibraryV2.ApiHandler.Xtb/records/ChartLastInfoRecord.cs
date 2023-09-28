using Newtonsoft.Json.Linq;
using RobotAppLibraryV2.ApiHandler.Xtb.codes;

namespace RobotAppLibraryV2.ApiHandler.Xtb.records;

using JSONObject = JObject;

public class ChartLastInfoRecord
{
    private readonly PERIOD_CODE period;
    private readonly long? start;
    private readonly string symbol;

    public ChartLastInfoRecord(string symbol, PERIOD_CODE period, long? start)
    {
        this.symbol = symbol;
        this.period = period;
        this.start = start;
    }

    public virtual JSONObject toJSONObject()
    {
        var obj = new JSONObject();
        obj.Add("symbol", symbol);
        obj.Add("period", (long?)period.Code);
        obj.Add("start", start);
        return obj;
    }
}