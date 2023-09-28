namespace RobotAppLibraryV2.ApiHandler.Xtb.errors;

public class APICommunicationException : Exception
{
    public APICommunicationException()
    {
    }

    public APICommunicationException(string msg) : base(msg)
    {
    }
}