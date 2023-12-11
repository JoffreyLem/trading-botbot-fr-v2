namespace RobotAppLibraryV2.Api.Xtb.Code;

public class STREAMING_TRADE_TYPE : BaseCode
{
    public static readonly STREAMING_TRADE_TYPE OPEN = new(0L);
    public static readonly STREAMING_TRADE_TYPE PENDING = new(1L);
    public static readonly STREAMING_TRADE_TYPE CLOSE = new(2L);

    public STREAMING_TRADE_TYPE(long code)
        : base(code)
    {
    }
}