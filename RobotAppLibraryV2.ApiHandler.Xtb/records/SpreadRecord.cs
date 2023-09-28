using Newtonsoft.Json.Linq;

namespace RobotAppLibraryV2.ApiHandler.Xtb.records;

using JSONObject = JObject;

public class SpreadRecord : BaseResponseRecord
{
    public virtual long? Precision { get; set; }

    public virtual string Symbol { get; set; }

    public virtual long? QuoteId { get; set; }

    public virtual long? Value { get; set; }

    public void FieldsFromJSONObject(JSONObject value)
    {
        Symbol = (string)value["symbol"];
        Precision = (long?)value["precision"];
        Value = (long?)value["value"];
        QuoteId = (long?)value["quoteId"];
    }
}