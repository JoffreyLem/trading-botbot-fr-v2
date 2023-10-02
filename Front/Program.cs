using Front;
using Front.Services;
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

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.AddDependency2();
app.MapRazorPages();
app.MapBlazorHub();




app.Run();