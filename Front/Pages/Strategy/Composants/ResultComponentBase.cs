using Microsoft.AspNetCore.Components;
using StrategyApi.StrategyBackgroundService.Dto.Services;

namespace Front.Pages.Strategy.Composants;

public class ResultComponentBase : StrategyIdComponentBase
{
    [Parameter] public ResultDto? ResultData { get; set; } = new();
}