using Microsoft.AspNetCore.Components;
using StrategyApi.StrategyBackgroundService.Dto.Services.Enum;
using StrategyApi.StrategyBackgroundService.Services;

namespace Front.Pages;

public class IndexBase : ComponentBase
{
    protected bool IsConnected;
    [Inject] protected IApiConnectService ApiConnectService { get; set; }

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