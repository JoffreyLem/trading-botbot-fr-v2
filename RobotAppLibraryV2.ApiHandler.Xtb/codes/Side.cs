namespace RobotAppLibraryV2.ApiHandler.Xtb.codes;

public class Side : BaseCode
{
    /// <summary>
    ///     Buy.
    /// </summary>
    public static readonly Side BUY = new(0);

    /// <summary>
    ///     Sell.
    /// </summary>
    public static readonly Side SELL = new(1);

    private Side(int code)
        : base(code)
    {
    }

    public Side FromCode(int code)
    {
        if (code == 0) return BUY;

        if (code == 1) return SELL;

        return null;
    }
}