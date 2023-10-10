using Front.Services;
using Microsoft.AspNetCore.Components;
using StrategyApi.StrategyBackgroundService.Dto.Services.Enum;
using StrategyApi.StrategyBackgroundService.Services;

namespace Front.Composants;

public class HeathCheckComponentBase : ComponentBase 
{
    [Inject]
    private IApiConnectService _ApiConnectService { get; set; }
    
    [Inject]
    private IStrategyHandlerService _apiStrategyService { get; set; }
    
    [Inject] private ShowToastService ToastService { get; set; }

    
    [Inject] private IEventBus _eventBus { get; set; }
    public ConnexionStateEnum Api { get; set; } = ConnexionStateEnum.Disconnected;
    public ConnexionStateEnum Strategy { get; set; } = ConnexionStateEnum.NotInitialized;
    
    protected override async Task OnInitializedAsync()
    {
        await InitializeStatus();
    }

    private async Task InitializeStatus()
    {
        try
        {
            await CheckApiHandlerState();
            await CheckStrategyState();
            _eventBus.Subscribe<ReferentEnum,ConnexionStateEnum>(BackgroundServiceConnectionStatusReceived);
            StateHasChanged();
        }
        catch (Exception)
        {
            ToastService.ShowToastError("Can't get status.");
        }
    }
    
    private void BackgroundServiceConnectionStatusReceived(ReferentEnum referentEnum, ConnexionStateEnum connexionStateEnum)
    {
        InvokeAsync(() =>
        {
            if (referentEnum == ReferentEnum.Api)
            {
                Api = connexionStateEnum;
            }
            else if (referentEnum == ReferentEnum.Strategy)
            {
                Strategy = connexionStateEnum;
            }
            StateHasChanged();
          
        });

    }
    
    
    private async Task CheckApiHandlerState()
    {
        try
        {
            Api = await _ApiConnectService.IsConnected();
        }
        catch (Exception e)
        {
            Api = ConnexionStateEnum.Inconnue;
        }
    }
    
    private async Task CheckStrategyState()
    {
        try
        {
            bool apiStrategyState = (await _apiStrategyService.IsInitialized()).Initialized;
            Strategy = apiStrategyState ? ConnexionStateEnum.Initialized : ConnexionStateEnum.NotInitialized;
        }
        catch (Exception e)
        {
            Strategy = ConnexionStateEnum.Inconnue;
        }
    }


}