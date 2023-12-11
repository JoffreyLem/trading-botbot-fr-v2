namespace StrategyApi.StrategyBackgroundService.Command.Strategy;

public class ServiceCommandBaseStrategy<T> : ServiceCommandeBaseStrategyAbstract where T : ServiceCommandResponse
{
    public TaskCompletionSource<T> ResponseSource { get; } = new();

    public override void SetException(System.Exception exception)
    {
        ResponseSource.SetException(exception);
    }
}