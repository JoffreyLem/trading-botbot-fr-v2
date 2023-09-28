namespace RobotAppLibraryV2.ApiHandler.Xtb.codes;

public class MARGIN_MODE : BaseCode
{
    public static readonly MARGIN_MODE FOREX = new(101L);
    public static readonly MARGIN_MODE CFD_LEVERAGED = new(102L);
    public static readonly MARGIN_MODE CFD = new(103L);

    public MARGIN_MODE(long code)
        : base(code)
    {
    }
}