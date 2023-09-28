namespace RobotAppLibraryV2.ApiHandler.Xtb.codes;

public class EXECUTION_CODE : BaseCode
{
    public static readonly EXECUTION_CODE EXE_REQUEST = new(0L);
    public static readonly EXECUTION_CODE EXE_INSTANT = new(1L);
    public static readonly EXECUTION_CODE EXE_MARKET = new(2L);

    public EXECUTION_CODE(long code)
        : base(code)
    {
    }
}