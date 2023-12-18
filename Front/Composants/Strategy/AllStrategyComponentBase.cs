using Front.Services;
using Microsoft.AspNetCore.Components;
using StrategyApi.StrategyBackgroundService.Dto.Services;
using StrategyApi.StrategyBackgroundService.Services;

namespace Front.Composants.Strategy;

public class AllStrategyComponentBase : ComponentBase
{
    protected List<StrategyInfoDto> StrategyList { get; set; } = new();

    [Inject] private IStrategyHandlerService _apiStrategyService { get; set; }

    [Inject] private ShowToastService ToastService { get; set; }


    protected override async Task OnInitializedAsync()
    {
        try
        {
            await InitializeStrategy();
        }
        catch (Exception)
        {
            ToastService.ShowToastError("Erreur d'initialisation des strategy");
        }
    }

    protected async Task InitializeStrategy()
    {
        try
        {
            var result = await _apiStrategyService.GetAllStrategy();
            if (result is { Count: > 0 }) StrategyList = result;
            this.StateHasChanged();
        }
        catch (Exception e)
        {
            ToastService.ShowToastError("Error on initialization");
        }
    }
}