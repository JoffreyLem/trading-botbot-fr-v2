using Front.Services;
using Microsoft.AspNetCore.Components;
using StrategyApi.StrategyBackgroundService;
using StrategyApi.StrategyBackgroundService.Dto.Services.Enum;
using StrategyApi.StrategyBackgroundService.Events;
using StrategyApi.StrategyBackgroundService.Services;

namespace Front.Composants;

public class HeathCheckComponentBase : ComponentBase, IDisposable
{
    [Inject] private IApiConnectService _ApiConnectService { get; set; }

    [Inject] private IStrategyHandlerService _apiStrategyService { get; set; }

    [Inject] private ShowToastService ToastService { get; set; }

    public ConnexionStateEnum Api { get; set; } = ConnexionStateEnum.Disconnected;
    public ConnexionStateEnum Strategy { get; set; } = ConnexionStateEnum.NotInitialized;


    public void Dispose()
    {
        CommandHandler.ConnexionState -= CommandHandlerOnConnexionState;
    }

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
            CommandHandler.ConnexionState += CommandHandlerOnConnexionState;
            StateHasChanged();
        }
        catch (Exception)
        {
            ToastService.ShowToastError("Can't get status.");
        }
    }

    private void CommandHandlerOnConnexionState(object? sender, ConnexionStateEventArgs e)
    {
        InvokeAsync(() =>
        {
            if (e.Referent == ReferentEnum.Api)
                Api = e.ConnexionState;
            else if (e.Referent == ReferentEnum.Strategy) Strategy = e.ConnexionState;
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
            var apiStrategyState = (await _apiStrategyService.IsInitialized()).Initialized;
            Strategy = apiStrategyState ? ConnexionStateEnum.Initialized : ConnexionStateEnum.NotInitialized;
        }
        catch (Exception e)
        {
            Strategy = ConnexionStateEnum.Inconnue;
        }
    }
}