using Auth0.AspNetCore.Authentication;
using Front;
using Front.Services;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.AspNetCore.HttpOverrides;
using StrategyApi.StrategyBackgroundService;
using StrategyApi.StrategyBackgroundService.Hubs;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSignalR();

builder.AddAuthentification();
builder.AddSyncFusion();
builder.AddDependencyRobot();
builder.AddLogger();
builder.Services.AddSingleton<ShowToastService>();
builder.Services.AddHealthChecks();


builder.WebHost.ConfigureAppConfiguration((ctx, cb) =>
    {
        if (!ctx.HostingEnvironment.IsDevelopment())
        {
            StaticWebAssetsLoader.UseStaticWebAssets(
                ctx.HostingEnvironment,
                ctx.Configuration);
        }
    }
);

var app = builder.Build();

var forwardedHeaderOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedHeaderOptions.KnownNetworks.Clear();
forwardedHeaderOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeaderOptions);


if (!app.Environment.IsDevelopment())
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
 //   app.UseHsts();
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();


app.UseAuthentication();
app.UseAuthorization();

app.AddDependency2();
app.MapRazorPages();
app.MapBlazorHub();

app.MapFallbackToPage("/_Host");




app.Run();