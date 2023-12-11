namespace RobotAppLibraryV2.ApiConnector.Exceptions;

public class ApiException : Exception
{
    public ApiException(string errorCode, string errorDescription)
        : base($"Error Code: {errorCode}, Description: {errorDescription}")
    {
        ErrorCode = errorCode;
        ErrorDescription = errorDescription;
    }

    public string ErrorCode { get; }
    public string ErrorDescription { get; }
}