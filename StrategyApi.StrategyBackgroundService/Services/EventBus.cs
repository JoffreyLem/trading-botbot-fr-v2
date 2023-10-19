using System.Reflection;

namespace StrategyApi.StrategyBackgroundService.Services;

// TODO : TU
public class EventBus : IEventBus
{
    private readonly Dictionary<object, List<Delegate>> _subscribers = new();

    public async Task PublishAsync<T>(T eventItem)
    {
        await InvokeHandlersAsync(typeof(T), eventItem);
    }

    public async Task PublishAsync<T1, T2>(T1 eventItem1, T2 eventItem2)
    {
        await InvokeHandlersAsync(Tuple.Create(typeof(T1), typeof(T2)), eventItem1, eventItem2);
    }

    public async Task PublishAsync<T1, T2, T3>(T1 eventItem1, T2 eventItem2, T3 eventItem3)
    {
        await InvokeHandlersAsync(Tuple.Create(typeof(T1), typeof(T2), typeof(T3)), eventItem1, eventItem2, eventItem3);
    }

    private async Task InvokeHandlersAsync(object key, params object[] eventItems)
    {
        if (_subscribers.TryGetValue(key, out var subscribers))
        {
            var tasks = subscribers.Select(subscriber => Task.Run(() => subscriber.DynamicInvoke(eventItems))).ToArray();
            await Task.WhenAll(tasks);
        }
    }

    public void Subscribe<T>(Action<T> action)
    {
        SubscribeInternal(typeof(T), action);
    }

    public void Subscribe<T1, T2>(Action<T1, T2> action)
    {
        SubscribeInternal(Tuple.Create(typeof(T1), typeof(T2)), action);
    }

    public void Subscribe<T1, T2, T3>(Action<T1, T2, T3> action)
    {
        SubscribeInternal(Tuple.Create(typeof(T1), typeof(T2), typeof(T3)), action);
    }
    
    public void Unsubscribe<T>(Action<T> action)
    {
        UnsubscribeInternal(typeof(T), action);
    }

    public void Unsubscribe<T1, T2>(Action<T1, T2> action)
    {
        UnsubscribeInternal(Tuple.Create(typeof(T1), typeof(T2)), action);
    }

    public void Unsubscribe<T1, T2, T3>(Action<T1, T2, T3> action)
    {
        UnsubscribeInternal(Tuple.Create(typeof(T1), typeof(T2), typeof(T3)), action);
    }

    private void SubscribeInternal(object key, Delegate action)
    {
        if (!_subscribers.TryGetValue(key, out var handlers))
        {
            handlers = new List<Delegate>();
            _subscribers[key] = handlers;
        }
        handlers.Add(action);
    }
    
    private void UnsubscribeInternal(object key, Delegate action)
    {
        if (_subscribers.TryGetValue(key, out var handlers))
        {
            handlers.Remove(action);
            if (handlers.Count == 0)
            {
                _subscribers.Remove(key);
            }
        }
    }
}