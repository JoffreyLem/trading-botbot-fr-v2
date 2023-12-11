using StrategyApi.StrategyBackgroundService.Dto.Services.Enum;

namespace StrategyApi.StrategyBackgroundService.Events;

public class StrategyEventEvent : EventArgs
{
    public EventType EventType { get; set; }

    public string Message { get; set; }
}