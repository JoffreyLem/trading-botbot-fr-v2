using System.Runtime.Serialization;

namespace RobotAppLibraryV2.ApiConnector.Exceptions;

public class ApiCommandCreationException : Exception
{
    public ApiCommandCreationException()
    {
    }

    protected ApiCommandCreationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public ApiCommandCreationException(string? message) : base(message)
    {
    }

    public ApiCommandCreationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}