using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace RobotAppLibraryV2.ApiHandler.Exceptions;

[ExcludeFromCodeCoverage]
public class ApiHandlerException : Exception
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

    public ApiHandlerException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}