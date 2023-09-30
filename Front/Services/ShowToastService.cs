using Front.Modeles;
using StrategyApi.StrategyBackgroundService.Dto.Services.Enum;

namespace Front.Services;

public class ShowToastService
{
    public event Action<ToastOptions>? ShowToastTrigger;

    public void ShowToast(EventType eventType, string message)
    {
        switch (eventType)
        {
            case EventType.Info:
                ShowToastInformation(message);
                break;
            case EventType.Warning:
                ShowToastWarning(message);
                break;
            case EventType.Error:
                ShowToastError(message);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
        }
    }


    public void ShowToastSuccess(string message)
    {
        ToastOptions options = new ToastOptions
        {
            Title = "Success",
            Content = message,
            CssClass = "e-toast-success"
        };
        ShowToastTrigger?.Invoke(options);
    }

    public void ShowToastInformation(string message)
    {
        ToastOptions options = new ToastOptions
        {
            Title = "Information",
            Content = message,
            CssClass = "e-toast-info"
        };
        ShowToastTrigger?.Invoke(options);
    }

    public void ShowToastWarning(string message)
    {
        ToastOptions options = new ToastOptions
        {
            Title = "Warning",
            Content = message,
            CssClass = "e-toast-warning"
        };
        ShowToastTrigger?.Invoke(options);
    }

    public void ShowToastError(string message)
    {
        ToastOptions options = new ToastOptions
        {
            Title = "Error",
            Content = message,
            CssClass = "e-toast-danger"
        };
        ShowToastTrigger?.Invoke(options);
    }

    public void ShowToastError(Exception ex)
    {
        ToastOptions options = new ToastOptions
        {
            Title = "Error",
            Content = ex.Message ?? "ERROR",
            CssClass = "e-toast-danger"
        };
        ShowToastTrigger?.Invoke(options);
    }
}