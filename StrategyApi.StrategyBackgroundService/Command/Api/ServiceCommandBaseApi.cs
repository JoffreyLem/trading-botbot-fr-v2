namespace StrategyApi.StrategyBackgroundService.Command.Api;

public class ServiceCommandBaseApi<T> : ServiceCommandeBaseApiAbstract where T : ServiceCommandResponse
{
    public TaskCompletionSource<T> ResponseSource { get; } = new();

    public override void SetException(System.Exception exception)
    {
        ResponseSource.SetException(exception);
    }
}