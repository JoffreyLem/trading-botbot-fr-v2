using Front.Modeles;
using Front.Services;
using Microsoft.AspNetCore.Components;
using StrategyApi.StrategyBackgroundService.Services;

namespace Front.Composants.Strategy;

public class StrategyComponentBase : ComponentBase
{
    protected bool IsInitialized;

    [Inject] private IStrategyHandlerService _apiStrategyService { get; set; }

    [Inject] private ShowToastService ToastService { get; set; }
    
    [Inject] private NavigationManager NavigationManager { get; set; }

    protected bool OnLoading { get; set; } = false;

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
       
    }
    
    protected async Task DeleteStrategy()
    {
        try
        {
            OnLoading = true;
            await _apiStrategyService.CloseStrategy();
            ToastService.ShowToastSuccess("Strategy supprimée");
            await InitializeStrategy();
        }
        catch (Exception e)
        {
            ToastService.ShowToastError(e);
        }
        finally
        {
            OnLoading = false;
        }
    }

    protected async Task HandleChildEvent()
    {
        await InitializeStrategy();
    }
    

}