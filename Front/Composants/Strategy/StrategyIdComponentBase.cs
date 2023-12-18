using Microsoft.AspNetCore.Components;

namespace Front.Composants.Strategy;

public class StrategyIdComponentBase : ComponentBase
{
    [CascadingParameter] public string StrategyId { get; set; }
}