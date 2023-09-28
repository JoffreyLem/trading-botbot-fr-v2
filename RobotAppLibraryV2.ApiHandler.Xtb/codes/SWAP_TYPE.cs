namespace RobotAppLibraryV2.ApiHandler.Xtb.codes;

public class SWAP_TYPE : BaseCode
{
    public static readonly SWAP_TYPE SWAP_BY_POINTS = new(0L);
    public static readonly SWAP_TYPE SWAP_BY_DOLLARS = new(1L);
    public static readonly SWAP_TYPE SWAP_BY_INTEREST = new(2L);
    public static readonly SWAP_TYPE SWAP_BY_MARGIN_CURRENCY = new(3L);

    public SWAP_TYPE(long code)
        : base(code)
    {
    }
}