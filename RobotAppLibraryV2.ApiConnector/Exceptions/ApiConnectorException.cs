using System.Runtime.Serialization;

namespace RobotAppLibraryV2.ApiConnector.Exceptions;

public class ApiConnectorException : Exception
{
    public ApiConnectorException()
    {
    }

    protected ApiConnectorException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public ApiConnectorException(string? message) : base(message)
    {
    }

    public ApiConnectorException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}