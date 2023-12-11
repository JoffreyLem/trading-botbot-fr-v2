using System.Runtime.Serialization;

namespace RobotAppLibraryV2.ApiConnector.Exceptions;

public class AdapterException : Exception
{
    public AdapterException()
    {
    }

    protected AdapterException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public AdapterException(string? message) : base(message)
    {
    }

    public AdapterException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}