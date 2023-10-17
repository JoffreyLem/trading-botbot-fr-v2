using AutoMapper;
using Front.Services;
using Microsoft.AspNetCore.Components;
using StrategyApi.StrategyBackgroundService.Dto.Services;
using StrategyApi.StrategyBackgroundService.Dto.Services.Enum;
using StrategyApi.StrategyBackgroundService.Hubs;
using StrategyApi.StrategyBackgroundService.Services;

namespace Front.Composants.Strategy;

public class StrategyDataBaseComponent : ComponentBase
{
    protected bool OnLoading;
    
    protected StrategyInfoDto StrategyInfo = new();
    protected ResultComponent ResultComponent { get; set; }

    [Inject] protected IStrategyHandlerService _strategyService { get; set; }
    [Inject] private ShowToastService ToastService { get; set; }
    [Inject] private IEventBus _eventBus { get; set; }
    
    [Parameter] public EventCallback StrategyCloseRequested { get; set; }

    protected int SelectedTab { get; set; } = 0;
    
    protected override async Task OnInitializedAsync()
    {
        try
        {
            StrategyInfo = await _strategyService.GetStrategyInfo();
            _eventBus.Subscribe<EventType,string>(StrategyHubOnOnEventReceived);
  
        }
        catch (Exception e)
        {
            ToastService.ShowToastError("Error strategy initialization");
        }
    }



    private void StrategyHubOnOnEventReceived(EventType eventType, string message)
    {
        InvokeAsync(async () =>
        {
            ToastService.ShowToast(eventType, message);
            if (eventType is EventType.Fatal or EventType.Close)
            {
                await StrategyCloseRequested.InvokeAsync();
            }

            StateHasChanged();
        });
    }
    
    protected async void OnCanRunChange(Syncfusion.Blazor.Buttons.ChangeEventArgs<bool?> args)
    {
        try
        {
            OnLoading = true;
            await _strategyService.SetCanRun(args.Checked.GetValueOrDefault());
            ToastService.ShowToastSuccess("Can run updated");
        }
        catch (Exception e)
        {
            ToastService.ShowToastError(e);
        }
        finally
        {
            OnLoading = false;
            StateHasChanged();
        }
    }

    protected async void OnSecureControlePositionChange(Syncfusion.Blazor.Buttons.ChangeEventArgs<bool> args)
    {
        try
        {
            OnLoading = true;
            await _strategyService.SetSecureControlPosition(args.Checked);
            ToastService.ShowToastSuccess("Secure control position updated");
        }
        catch (Exception e)
        {
            ToastService.ShowToastError(e);
        }
        finally
        {
            OnLoading = false;
            StateHasChanged();
        }
    }
}