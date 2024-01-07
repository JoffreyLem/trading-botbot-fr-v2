using DotNetEnv;
using Front.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using StrategyApi.DataBase;
using StrategyApi.StrategyBackgroundService;
using StrategyApi.StrategyBackgroundService.Mapper;
using Syncfusion.Blazor;
using Syncfusion.Licensing;

var builder = WebApplication.CreateBuilder(args);
Env.Load();
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));
builder.Services.AddControllersWithViews(options =>
    {
        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
        options.Filters.Add(new AuthorizeFilter(policy));
    })
    .AddMicrosoftIdentityUI();
builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy
    options.FallbackPolicy = options.DefaultPolicy;
});
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor().AddMicrosoftIdentityConsentHandler();
builder.Services.AddSignalR();

SyncfusionLicenseProvider.RegisterLicense(
    "Ngo9BigBOggjHTQxAR8/V1NAaF5cWWJCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdgWH1ccnRUQ2BZVkNzX0Q=");
builder.Services.AddSyncfusionBlazor();

builder.AddBotDependency();

builder.Services.AddAutoMapper(cfg => { cfg.AddProfile<MappingProfilesBackgroundServices>(); },
    typeof(MappingProfilesBackgroundServices).Assembly
);

builder.Services.AddSignalR();
builder.AddLogger();
builder.Services.AddSingleton<ShowToastService>();
builder.Services.AddHealthChecks();
builder.Services.AddStrategyDbContext(builder.Configuration);

builder.WebHost.ConfigureAppConfiguration((ctx, cb) =>
    {
        if (!ctx.HostingEnvironment.IsDevelopment())
            StaticWebAssetsLoader.UseStaticWebAssets(
                ctx.HostingEnvironment,
                ctx.Configuration);
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

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
    app.UseHsts();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();


app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.MapControllers();
app.MapBlazorHub();

app.MapFallbackToPage("/_Host");

app.Run();