using MudBlazor.Services;
using Syncfusion.Blazor;
using Syncfusion.Licensing;

namespace RobotBlazorApp;

public static class ConfigurationProgramHelper
{
    public static void ConfigureSyncfusion(this WebApplicationBuilder builder)
    {
        SyncfusionLicenseProvider.RegisterLicense(
            "ODE1NjAyQDMyMzAyZTM0MmUzMG5aWTRQa0Rwb3d1b2s3eWEvOERxUnRqUEtzVWNwV3QrMkQ3NnVBL2c3T1k9");
        builder.Services.AddSyncfusionBlazor();
    }
    
    public static void ConfigureMudBlazor(this WebApplicationBuilder builder)
    {
        builder.Services.AddMudServices();
    }
    
}