namespace RobotAppLibraryV2.ApiHandler.Xtb.codes;

public class PROFIT_MODE : BaseCode
{
    public static readonly PROFIT_MODE FOREX = new(5L);
    public static readonly PROFIT_MODE CFD = new(6L);

    public PROFIT_MODE(long code)
        : base(code)
    {
    }
}