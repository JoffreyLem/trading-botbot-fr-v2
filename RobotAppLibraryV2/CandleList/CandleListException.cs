using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace RobotAppLibraryV2.CandleList;

[ExcludeFromCodeCoverage]
public class CandleListException : Exception
{
    public CandleListException()
    {
    }

    protected CandleListException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public CandleListException(string? message) : base(message)
    {
    }

    public CandleListException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}