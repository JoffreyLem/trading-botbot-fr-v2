using Newtonsoft.Json.Linq;

namespace RobotAppLibraryV2.ApiHandler.Xtb.records;

using JSONObject = JObject;

public class RateInfoRecord : BaseResponseRecord
{
    public virtual long? Ctm { get; set; }

    public virtual double? Open { get; set; }

    public virtual double? High { get; set; }

    public virtual double? Low { get; set; }

    public virtual double? Close { get; set; }

    public virtual double? Vol { get; set; }

    public void FieldsFromJSONObject(JSONObject value)
    {
        {
            Close = (double?)value["close"];
            Ctm = (long?)value["ctm"];
            High = (double?)value["high"];
            Low = (double?)value["low"];
            Open = (double?)value["open"];
            Vol = (double?)value["vol"];
        }
    }
}