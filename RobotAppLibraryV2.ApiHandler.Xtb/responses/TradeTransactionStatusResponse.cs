using Newtonsoft.Json.Linq;
using RobotAppLibraryV2.ApiHandler.Xtb.codes;

namespace RobotAppLibraryV2.ApiHandler.Xtb.responses;

using JSONObject = JObject;

public class TradeTransactionStatusResponse : BaseResponse
{
    private double? ask;
    private double? bid;
    private string customComment;
    private string message;
    private long? order;
    private REQUEST_STATUS requestStatus;

    public TradeTransactionStatusResponse(string body) : base(body)
    {
        var ob = (JSONObject)ReturnData;
        ask = (double?)ob["ask"];
        bid = (double?)ob["bid"];
        customComment = (string)ob["customComment"];
        message = (string)ob["message"];
        order = (long?)ob["order"];
        requestStatus = new REQUEST_STATUS((long)ob["requestStatus"]);
    }

    public virtual double? Ask
    {
        get => ask;

        set => ask = value;
    }

    public virtual double? Bid
    {
        get => bid;
        set => bid = value;
    }

    public virtual string CustomComment
    {
        get => customComment;
        set => customComment = value;
    }

    public virtual string Message
    {
        get => message;
        set => message = value;
    }

    public virtual long? Order
    {
        get => order;
        set => order = value;
    }

    public virtual REQUEST_STATUS RequestStatus
    {
        get => requestStatus;
        set => requestStatus = value;
    }
}