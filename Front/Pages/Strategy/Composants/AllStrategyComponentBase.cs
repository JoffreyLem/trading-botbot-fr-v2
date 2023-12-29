using Front.Services;
using Microsoft.AspNetCore.Components;
using StrategyApi.StrategyBackgroundService.Dto.Services;
using StrategyApi.StrategyBackgroundService.Services;
using Syncfusion.Blazor.Navigations;

namespace Front.Pages.Strategy.Composants;

public class AllStrategyComponentBase : ComponentBase
{
    protected List<StrategyInfoDto> StrategyList { get; set; } = new();
    protected int SelectedTab { get; set; } = 0;
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
            StrategyList = result is { Count: > 0 } ? result : new List<StrategyInfoDto>();
            StateHasChanged();
        }
        catch (Exception e)
        {
            ToastService.ShowToastError("Error on initialization");
        }
    }
    
    protected async Task Callback()
    {
       
    }
}