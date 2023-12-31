using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace RobotAppLibraryV2.Results;

[ExcludeFromCodeCoverage]
public class ResultException : Exception
{
    public ResultException()
    {
    }

    protected ResultException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public ResultException(string? message) : base(message)
    {
    }

    public ResultException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}