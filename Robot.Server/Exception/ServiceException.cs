using System.Runtime.Serialization;

namespace Robot.Server.Exception;

public class ServiceException : System.Exception
{
    public ServiceException()
    {
    }

    protected ServiceException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public ServiceException(string? message) : base(message)
    {
    }

    public ServiceException(string? message, System.Exception? innerException) : base(message, innerException)
    {
    }
}