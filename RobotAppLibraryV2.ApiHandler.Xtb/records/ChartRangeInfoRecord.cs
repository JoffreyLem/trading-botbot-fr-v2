using Newtonsoft.Json.Linq;
using RobotAppLibraryV2.ApiHandler.Xtb.codes;

namespace RobotAppLibraryV2.ApiHandler.Xtb.records;

using JSONObject = JObject;

public class ChartRangeInfoRecord
{
    private readonly long? end;
    private readonly PERIOD_CODE period;
    private readonly long? start;

    private readonly string symbol;
    private readonly long? ticks;

    public ChartRangeInfoRecord(string symbol, PERIOD_CODE period, long? start, long? end, long? ticks)
    {
        this.symbol = symbol;
        this.period = period;
        this.start = start;
        this.end = end;
        this.ticks = ticks;
    }

    public virtual JSONObject toJSONObject()
    {
        var obj = new JSONObject();
        obj.Add("symbol", symbol);
        obj.Add("period", (long?)period.Code);
        obj.Add("start", start);
        obj.Add("end", end);
        obj.Add("ticks", ticks);
        return obj;
    }
}