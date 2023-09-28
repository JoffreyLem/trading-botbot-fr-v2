using RobotAppLibraryV2.ApiHandler.Xtb.errors;

namespace RobotAppLibraryV2.ApiHandler.Xtb.responses;

public class APIErrorResponse : Exception
{
    private readonly ERR_CODE code;
    private readonly string errDesc;
    private readonly string msg;

    public APIErrorResponse(ERR_CODE code, string errDesc, string msg) : base(msg)
    {
        this.code = code;
        this.errDesc = errDesc;
        this.msg = msg;
    }

    public override string Message =>
        "ERR_CODE = " + code.StringValue + " ERR_DESC = " + errDesc + "\n" + msg + "\n" + base.Message;

    public virtual string Msg => msg;

    public virtual ERR_CODE ErrorCode => code;

    public virtual string ErrorDescr => errDesc;

    public override string ToString()
    {
        return ErrorCode.StringValue + ": " + ErrorDescr;
    }
}