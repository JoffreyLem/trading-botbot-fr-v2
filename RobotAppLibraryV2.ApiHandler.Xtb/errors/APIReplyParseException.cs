namespace RobotAppLibraryV2.ApiHandler.Xtb.errors;

public class APIReplyParseException : Exception
{
    public APIReplyParseException()
    {
    }

    public APIReplyParseException(string msg) : base(msg)
    {
    }
}