using Front.Services;
using Microsoft.AspNetCore.Components;
using StrategyApi.StrategyBackgroundService.Services;

namespace Front.Composants.Strategy;

public class StrategyComponentBase : ComponentBase
{
    protected bool IsInitialized;

    [Inject] private IStrategyHandlerService _apiStrategyService { get; set; }

    [Inject] private ShowToastService ToastService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await InitializeStrategy();
    }

    private async Task InitializeStrategy()
    {
        try
        {
            IsInitialized = (await _apiStrategyService.IsInitialized()).Initialized;
        }
        catch (Exception e)
        {
            ToastService.ShowToastError("Error on initialization");
        }
        finally
        {
            StateHasChanged();
        }
    }

    protected async Task HandleStrategyForm()
    {
        await InitializeStrategy();
    }
}