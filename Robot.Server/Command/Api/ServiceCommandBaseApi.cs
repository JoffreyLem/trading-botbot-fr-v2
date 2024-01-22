namespace Robot.Server.Command.Api;

public class ServiceCommandBaseApi<T> : ServiceCommandeBaseApiAbstract where T : ServiceCommandResponseBase
{
    public TaskCompletionSource<T> ResponseSource { get; } = new();

    public override void SetException(System.Exception exception)
    {
        ResponseSource.SetException(exception);
    }
}