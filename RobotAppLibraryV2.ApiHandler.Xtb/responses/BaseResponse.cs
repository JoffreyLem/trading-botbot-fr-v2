using Newtonsoft.Json.Linq;
using RobotAppLibraryV2.ApiHandler.Xtb.errors;

namespace RobotAppLibraryV2.ApiHandler.Xtb.responses;

using JSONAware = JContainer;
using JSONObject = JObject;

public class BaseResponse
{
    private readonly ERR_CODE errCode;
    private readonly string errorDescr;
    private readonly JSONAware returnData;
    private readonly bool? status;

    public BaseResponse(string body)
    {
        JSONObject ob;
        try
        {
            ob = JSONObject.Parse(body);
        }
        catch (Exception x)
        {
            throw new APIReplyParseException("JSON Parse exception: " + body + "\n" + x.Message);
        }

        if (ob == null) throw new APIReplyParseException("JSON Parse exception: " + body);

        status = (bool?)ob["status"];
        errCode = new ERR_CODE((string)ob["errorCode"]);
        errorDescr = (string)ob["errorDescr"];
        returnData = (JSONAware)ob["returnData"];
        CustomTag = (string)ob["customTag"];

        if (status == null)
        {
            Console.Error.WriteLine(body);
            throw new APIReplyParseException("JSON Parse error: " + "\"status\" is null!");
        }

        if (status == null || (bool)!status)
            // If status is false check if redirect exists in given response
            if (ob["redirect"] == null)
            {
                if (errorDescr == null) errorDescr = ERR_CODE.getErrorDescription(errCode.StringValue);

                throw new APIErrorResponse(errCode, errorDescr, body);
            }
    }

    public virtual object ReturnData => returnData;

    public virtual bool? Status => status;

    public virtual string ErrorDescr => errorDescr;

    public string CustomTag { get; }

    public string ToJSONString()
    {
        var obj = new JSONObject();
        obj.Add("status", status);

        if (returnData != null) obj.Add("returnData", returnData.ToString());

        if (errCode != null) obj.Add("errorCode", errCode.StringValue);

        if (errorDescr != null) obj.Add("errorDescr", errorDescr);

        if (CustomTag != null) obj.Add("customTag", CustomTag);

        return obj.ToString();
    }
}