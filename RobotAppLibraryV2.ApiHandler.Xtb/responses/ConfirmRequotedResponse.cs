using Newtonsoft.Json.Linq;

namespace RobotAppLibraryV2.ApiHandler.Xtb.responses;

using JSONObject = JObject;

public class ConfirmRequotedResponse : BaseResponse
{
    public ConfirmRequotedResponse(string body) : base(body)
    {
        var ob = (JSONObject)ReturnData;
        NewRequestId = (long?)ob["requestId"];
    }

    public virtual long? NewRequestId { get; }
}