using Newtonsoft.Json.Linq;

namespace RobotAppLibraryV2.ApiHandler.Xtb.responses;

using JSONObject = JObject;

public class MarginLevelResponse : BaseResponse
{
    public MarginLevelResponse(string body) : base(body)
    {
        var ob = (JSONObject)ReturnData;
        Balance = (double?)ob["balance"];
        Equity = (double?)ob["equity"];
        Currency = (string)ob["currency"];
        Margin = (double?)ob["margin"];
        Margin_free = (double?)ob["margin_free"];
        Margin_level = (double?)ob["margin_level"];
        Credit = (double?)ob["credit"];
    }

    public virtual double? Balance { get; }

    public virtual double? Equity { get; }

    public virtual double? Margin { get; }

    public virtual double? Margin_free { get; }

    public virtual double? Margin_level { get; }

    public virtual string Currency { get; }

    public virtual double? Credit { get; }
}