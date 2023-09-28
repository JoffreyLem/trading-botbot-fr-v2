using Newtonsoft.Json.Linq;
using RobotAppLibraryV2.ApiHandler.Xtb.codes;

namespace RobotAppLibraryV2.ApiHandler.Xtb.records;

using JSONObject = JObject;

public class StreamingTradeStatusRecord : BaseResponseRecord
{
    public string CustomComment { get; set; }

    public string Message { get; set; }

    public long? Order { get; set; }

    public double? Price { get; set; }

    public REQUEST_STATUS RequestStatus { get; set; }

    public void FieldsFromJSONObject(JSONObject value)
    {
        CustomComment = (string)value["customComment"];
        Message = (string)value["message"];
        Order = (long?)value["order"];
        Price = (double?)value["price"];
        RequestStatus = new REQUEST_STATUS((long)value["requestStatus"]);
    }

    public override string ToString()
    {
        return "StreamingTradeStatusRecord{" +
               "customComment=" + CustomComment +
               "message=" + Message +
               ", order=" + Order +
               ", requestStatus=" + RequestStatus.Code +
               ", price=" + Price +
               '}';
    }
}