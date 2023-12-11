using System.Runtime.Serialization;

namespace RobotAppLibraryV2.ApiConnector.Exceptions;

public class ApiCommunicationException : Exception
{
    public ApiCommunicationException()
    {
    }

    protected ApiCommunicationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public ApiCommunicationException(string? message) : base(message)
    {
    }

    public ApiCommunicationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}