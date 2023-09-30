using Microsoft.AspNetCore.Components;
using StrategyApi.Dto.Enum;
using StrategyApi.StrategyBackgroundService.Services;

namespace Front.Pages;

public class IndexBase : ComponentBase
{
    [Inject] protected IApiConnectService ApiConnectService { get; set; }

    protected bool IsConnected = false;

    protected override async Task OnInitializedAsync()
    {
        await LoadConnectionState();
    }
    
    protected async Task HandleApiUpdate()
    {
        await LoadConnectionState();
    }
    
    private async Task LoadConnectionState()
    {
        var isConnected = await ApiConnectService.IsConnected();

        IsConnected = isConnected == ConnexionStateEnum.Connected;

        StateHasChanged();  
    }

}