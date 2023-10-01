using Microsoft.JSInterop;

namespace ApiFront;

public static class Utils
{
    public static async Task<string> ConvertToLocalTimeAsync(IJSRuntime jsRuntime, DateTime utcDate)
    {
        return await jsRuntime.InvokeAsync<string>("convertToLocalTime", utcDate);
    }
}