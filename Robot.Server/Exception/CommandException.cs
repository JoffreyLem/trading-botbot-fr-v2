namespace Robot.Server.Exception;

public class CommandException : System.Exception
{
    public CommandException(string? message) : base(message)
    {
    }
}