namespace RobotAppLibraryV2.ApiHandler.Xtb.errors;

public class APICommandConstructionException : Exception
{
    public APICommandConstructionException()
    {
    }

    public APICommandConstructionException(string msg) : base(msg)
    {
    }
}