namespace StrategyApi.StrategyBackgroundService.Services;

public interface IEventBus
{
    Task PublishAsync<T>(T eventItem);
    Task PublishAsync<T1, T2>(T1 eventItem1, T2 eventItem2);
    Task PublishAsync<T1, T2, T3>(T1 eventItem1, T2 eventItem2, T3 eventItem3);

    void Subscribe<T>(Action<T> action);
    void Subscribe<T1, T2>(Action<T1, T2> action);
    void Subscribe<T1, T2, T3>(Action<T1, T2, T3> action);

    public void Unsubscribe<T>(Action<T> action);

    public void Unsubscribe<T1, T2>(Action<T1, T2> action);

    public void Unsubscribe<T1, T2, T3>(Action<T1, T2, T3> action);
}