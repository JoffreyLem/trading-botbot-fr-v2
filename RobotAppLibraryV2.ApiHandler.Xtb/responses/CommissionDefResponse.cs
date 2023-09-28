using Newtonsoft.Json.Linq;

namespace RobotAppLibraryV2.ApiHandler.Xtb.responses;

using JSONObject = JObject;

public class CommissionDefResponse : BaseResponse
{
    public CommissionDefResponse(string body) : base(body)
    {
        var rd = (JSONObject)ReturnData;
        Commission = (double?)rd["commission"];
        RateOfExchange = (double?)rd["rateOfExchange"];
    }

    public virtual double? Commission { get; }

    public virtual double? RateOfExchange { get; }
}