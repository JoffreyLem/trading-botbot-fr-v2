namespace StrategyApi.StrategyBackgroundService.Exception;

public class CommandException : System.Exception
{
    public CommandException(string? message) : base(message) { }
}