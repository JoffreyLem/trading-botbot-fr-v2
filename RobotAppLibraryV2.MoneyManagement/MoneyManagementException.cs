using System.Diagnostics.CodeAnalysis;

namespace RobotAppLibraryV2.MoneyManagement;

[ExcludeFromCodeCoverage]
public class MoneyManagementException : Exception
{
    public MoneyManagementException(string? message) : base(message)
    {
    }

    public MoneyManagementException(string? message, Exception? innerException) :
        base(message, innerException)
    {
    }
}