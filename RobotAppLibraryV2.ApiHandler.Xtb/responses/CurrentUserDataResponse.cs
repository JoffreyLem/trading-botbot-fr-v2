using Newtonsoft.Json.Linq;

namespace RobotAppLibraryV2.ApiHandler.Xtb.responses;

using JSONObject = JObject;

public class CurrentUserDataResponse : BaseResponse
{
    private readonly long? leverage;

    public CurrentUserDataResponse(string body)
        : base(body)
    {
        var ob = (JSONObject)ReturnData;
        Currency = (string)ob["currency"];
        leverage = (long?)ob["leverage"];
        LeverageMultiplier = (double?)ob["leverageMultiplier"];
        Group = (string)ob["group"];
        CompanyUnit = (int?)ob["companyUnit"];
        SpreadType = (string)ob["spreadType"];
        IbAccount = (bool?)ob["ibAccount"];
    }

    public virtual string Currency { get; }

    [Obsolete("Use LeverageMultiplier instead")]
    public virtual long? Leverage => leverage;

    public virtual double? LeverageMultiplier { get; }

    public virtual string Group { get; }

    public virtual int? CompanyUnit { get; }

    public virtual string SpreadType { get; }

    public virtual bool? IbAccount { get; }
}