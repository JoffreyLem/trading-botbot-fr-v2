using Serilog.Core;
using Serilog.Events;

namespace StrategyApi.StrategyBackgroundService;

public class RemovePropertiesEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent le, ILogEventPropertyFactory lepf)
    {
        le.RemovePropertyIfPresent("RequestId");
        le.RemovePropertyIfPresent("RequestPath");
        le.RemovePropertyIfPresent("ActionName");
        le.RemovePropertyIfPresent("ActionId");
    }
}