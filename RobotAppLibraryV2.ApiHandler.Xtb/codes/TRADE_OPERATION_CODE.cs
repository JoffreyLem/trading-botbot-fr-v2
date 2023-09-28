namespace RobotAppLibraryV2.ApiHandler.Xtb.codes;

public class TRADE_OPERATION_CODE : BaseCode
{
    public static readonly TRADE_OPERATION_CODE BUY = new(0L);
    public static readonly TRADE_OPERATION_CODE SELL = new(1L);
    public static readonly TRADE_OPERATION_CODE BUY_LIMIT = new(2L);
    public static readonly TRADE_OPERATION_CODE SELL_LIMIT = new(3L);
    public static readonly TRADE_OPERATION_CODE BUY_STOP = new(4L);
    public static readonly TRADE_OPERATION_CODE SELL_STOP = new(5L);
    public static readonly TRADE_OPERATION_CODE BALANCE = new(6L);

    public TRADE_OPERATION_CODE(long code)
        : base(code)
    {
    }

    public override string ToString()
    {
        return Code.ToString();
    }
}