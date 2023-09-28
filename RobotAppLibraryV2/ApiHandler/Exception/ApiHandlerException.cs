using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace RobotAppLibraryV2.ApiHandler.Exception;

[ExcludeFromCodeCoverage]
public class ApiHandlerException : System.Exception
{
    public ApiHandlerException()
    {
    }

    protected ApiHandlerException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public ApiHandlerException(string? message) : base(message)
    {
    }

    public ApiHandlerException(string? message, System.Exception? innerException) : base(message, innerException)
    {
    }
}