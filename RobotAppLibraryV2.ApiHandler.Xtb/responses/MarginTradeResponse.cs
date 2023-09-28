using Newtonsoft.Json.Linq;

namespace RobotAppLibraryV2.ApiHandler.Xtb.responses;

using JSONObject = JObject;

public class MarginTradeResponse : BaseResponse
{
    public MarginTradeResponse(string body) : base(body)
    {
        var ob = (JSONObject)ReturnData;
        Margin = (double?)ob["margin"];
    }

    public virtual double? Margin { get; }
}