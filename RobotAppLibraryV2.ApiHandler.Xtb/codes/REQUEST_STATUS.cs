namespace RobotAppLibraryV2.ApiHandler.Xtb.codes;

public class REQUEST_STATUS : BaseCode
{
    public static readonly REQUEST_STATUS ERROR = new(0L);
    public static readonly REQUEST_STATUS PENDING = new(1L);
    public static readonly REQUEST_STATUS ACCEPTED = new(3L);
    public static readonly REQUEST_STATUS REJECTED = new(4L);

    public REQUEST_STATUS(long code)
        : base(code)
    {
    }
}