using Microsoft.AspNetCore.Components;

namespace Front.Pages.Strategy.Composants;

public class StrategyIdComponentBase : ComponentBase
{
    [CascadingParameter] public string StrategyId { get; set; }
}