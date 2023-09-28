namespace RobotAppLibraryV2.ApiHandler.Xtb.codes;

public class TRADE_TRANSACTION_TYPE : BaseCode
{
    public static readonly TRADE_TRANSACTION_TYPE ORDER_OPEN = new(0L);
    public static readonly TRADE_TRANSACTION_TYPE ORDER_CLOSE = new(2L);
    public static readonly TRADE_TRANSACTION_TYPE ORDER_MODIFY = new(3L);
    public static readonly TRADE_TRANSACTION_TYPE ORDER_DELETE = new(4L);

    public TRADE_TRANSACTION_TYPE(long code)
        : base(code)
    {
    }

    public override string ToString()
    {
        return Code.ToString();
    }
}