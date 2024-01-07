using Front.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using RobotAppLibraryV2.ApiHandler.Handlers.Enum;
using StrategyApi.StrategyBackgroundService.Dto;
using StrategyApi.StrategyBackgroundService.Dto.Enum;
using StrategyApi.StrategyBackgroundService.Services;
using Syncfusion.Blazor.DropDowns;

namespace Front.Pages.Api;

public class ApiIndexBase : ComponentBase, IDisposable
{
    protected bool IsConnected { get; set; }
    protected bool OnLoading { get; set; }

    protected bool ApiHandlerListEnabled { get; set; }
    protected SfDropDownList<string, string> _dropDownList { get; set; }
    [Inject] private IApiConnectService ApiConnectService { get; set; }
    [Inject] private ShowToastService ToastService { get; set; }
    protected List<string> ApiProviders { get; set; } = new();
    protected ConnectDto ConnectDto { get; set; } = new();
    protected string? ApiSelected { get; set; } = "";

    public void Dispose()
    {
    }


    protected override async Task OnInitializedAsync()
    {
        try
        {
            OnLoading = true;
            if (await ApiConnectService.IsConnected() == ConnexionStateEnum.Connected)
            {
                IsConnected = true;
                ApiHandlerListEnabled = false;
            }
            else
            {
                ApiHandlerListEnabled = true;
            }

            await GetListHandler();
        }
        catch (Exception e)
        {
            ToastService.ShowToastError(e.Message);
        }
        finally
        {
            OnLoading = false;
        }
    }


    private async Task GetListHandler()
    {
        try
        {
            ApiProviders = await ApiConnectService.GetListHandler();
            if (IsConnected)
            {
                var data = await ApiConnectService.GetTypeHandler();
                data = data.Replace("\"", "");
                ApiSelected = ApiProviders.First(x => x == data);
            }
        }
        catch (Exception e)
        {
            ToastService.ShowToastError(e);
        }
    }


    protected async Task Disconnect()
    {
        try
        {
            if (await ApiConnectService.IsConnected() == ConnexionStateEnum.Connected)
            {
                OnLoading = true;
                await ApiConnectService.Disconnect();
                ToastService.ShowToastSuccess("Deconnecter");
                IsConnected = false;
                ApiSelected = null;
                ApiHandlerListEnabled = true;
            }
            else
            {
                ToastService.ShowToastWarning("API Non connecter");
            }
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


    protected async Task Connect(EditContext obj)
    {
        try
        {
            OnLoading = true;
            await ApiConnectService.Connect(ConnectDto.User, ConnectDto.Pwd);
            ToastService.ShowToastSuccess("Connecter");
            IsConnected = true;
            ApiHandlerListEnabled = false;
            ConnectDto = new ConnectDto();
        }
        catch (Exception)
        {
            OnLoading = false;
            ToastService.ShowToastError("Erreur de connexion");
        }
        finally
        {
            OnLoading = false;
        }
    }

    protected async Task OnApiProviderValueChange(ChangeEventArgs<string, string> obj)
    {
        try
        {
            if (obj.IsInteracted && !IsConnected)
            {
                var apiHandlerEnum = Enum.Parse<ApiHandlerEnum>(obj.Value);
                await ApiConnectService.InitHandler(apiHandlerEnum);
                ToastService.ShowToastSuccess($"Handler set to {obj.Value}");
            }
        }
        catch (Exception e)
        {
            ApiSelected = obj.PreviousItemData;
            ToastService.ShowToastError(e);
        }
    }
}