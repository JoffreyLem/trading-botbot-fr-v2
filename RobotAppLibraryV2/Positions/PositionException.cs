using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace RobotAppLibraryV2.Positions;

[ExcludeFromCodeCoverage]
public class PositionException : Exception
{
    public PositionException()
    {
    }

    protected PositionException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public PositionException(string? message) : base(message)
    {
    }

    public PositionException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}