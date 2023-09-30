using System.Runtime.Serialization;

namespace StrategyApi.StrategyBackgroundService.Exception;

public class CommandExceptionInternalError : System.Exception
{
    public CommandExceptionInternalError()
    {
    }

    protected CommandExceptionInternalError(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public CommandExceptionInternalError(string? message) : base(message)
    {
    }

    public CommandExceptionInternalError(string? message, System.Exception? innerException) : base(message,
        innerException)
    {
    }
}