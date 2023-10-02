using Microsoft.AspNetCore.Components;
using StrategyApi.StrategyBackgroundService.Dto.Services.Enum;
using StrategyApi.StrategyBackgroundService.Services;

namespace Front.Composants;

public class HeathCheckComponentBase : ComponentBase , IDisposable, IAsyncDisposable
{
    [Inject]
    private IApiConnectService _ApiConnectService { get; set; }
    
    [Inject]
    private IStrategyHandlerService _apiStrategyService { get; set; }
    
    private Timer _statusCheckTimer;
    
    public ConnexionStateEnum Api { get; set; } = ConnexionStateEnum.Disconnected;
    public ConnexionStateEnum Strategy { get; set; } = ConnexionStateEnum.Disconnected;
    
    protected override async Task OnInitializedAsync()
    {
        await InitializeStatus();
        
        _statusCheckTimer = new Timer(async _ => await InvokeAsync(InitializeStatus), null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));

    }

    private async Task InitializeStatus()
    {
        try
        {
            await CheckApiHandlerState();
            await CheckStrategyState();
            StateHasChanged();
        }
        catch (Exception) { }
    }
    
    
    private async Task CheckApiHandlerState()
    {
        try
        {
            Api = await _ApiConnectService.IsConnected();
        }
        catch (Exception e)
        {
            Api = ConnexionStateEnum.Disconnected;
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
            Strategy = ConnexionStateEnum.Disconnected;
        }
    }

    public void Dispose()
    {
        _statusCheckTimer.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _statusCheckTimer.DisposeAsync();
    }
}