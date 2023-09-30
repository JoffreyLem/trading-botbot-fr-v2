using Front.Services;
using Microsoft.AspNetCore.Components;
using StrategyApi.StrategyBackgroundService.Dto.Services;
using StrategyApi.StrategyBackgroundService.Services;

namespace Front.Composants.Strategy;

public class ResultComponentBase : ComponentBase
{
    protected ResultDto? ResultData { get; set; } = new ResultDto();

    [Inject] protected IStrategyHandlerService _strategyService { get; set; }

    [Inject] private ShowToastService ToastService { get; set; }


    protected override async Task OnInitializedAsync()
    {
        try
        {
            ResultData = await GetResults();
        }
        catch (Exception)
        {
            ToastService.ShowToastError("Can't load result");
        }
    }

    private async Task<ResultDto?> GetResults()
    {
        ResultDto data = await _strategyService.GetResult();

        return data;
    }
}