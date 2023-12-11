namespace StrategyApi.StrategyBackgroundService.Command;

public abstract class ServiceCommandBase<T> : ServiceCommandeBaseAbstract where T : ServiceCommandResponse
{
    public TaskCompletionSource<T> ResponseSource { get; } = new();

    public override void SetException(System.Exception exception)
    {
        ResponseSource.SetException(exception);
    }
}