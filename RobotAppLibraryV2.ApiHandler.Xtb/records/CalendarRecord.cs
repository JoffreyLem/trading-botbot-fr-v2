using Newtonsoft.Json.Linq;

namespace RobotAppLibraryV2.ApiHandler.Xtb.records;

using JSONObject = JObject;

public class CalendarRecord : BaseResponseRecord
{
    public string Country { get; private set; }

    public string Current { get; private set; }

    public string Forecast { get; private set; }

    public string Impact { get; private set; }

    public string Period { get; private set; }

    public string Previous { get; private set; }

    public long? Time { get; private set; }

    public string Title { get; private set; }

    public void FieldsFromJSONObject(JSONObject value)
    {
        Country = (string)value["country"];
        Current = (string)value["current"];
        Forecast = (string)value["forecast"];
        Impact = (string)value["impact"];
        Period = (string)value["period"];
        Previous = (string)value["previous"];
        Time = (long?)value["time"];
        Title = (string)value["title"];
    }

    public override string ToString()
    {
        return "CalendarRecord[" + "country=" + Country + ", current=" + Current + ", forecast=" + Forecast +
               ", impact=" + Impact + ", period=" + Period + ", previous=" + Previous + ", time=" + Time + ", title=" +
               Title + "]";
    }
}