using Auth0.AspNetCore.Authentication;
using Destructurama;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Elasticsearch;
using Serilog.Sinks.Elasticsearch;
using Syncfusion.Blazor;
using Syncfusion.Licensing;

namespace Front;

public static class ProgramConfigurationHelper
{
    public static void AddSyncFusion(this WebApplicationBuilder builder)
    {
        SyncfusionLicenseProvider.RegisterLicense(
            "Ngo9BigBOggjHTQxAR8/V1NHaF5cXmtCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdgWXZceHVURmFfVkNwWEI=");
        builder.Services.AddSyncfusionBlazor();
    }

    public static void AddAuthentification(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddOpenIdConnect("Auth0", options =>
            {
                options.Authority = $"https://{builder.Configuration["Auth0:Domain"]}";
        
                options.ClientId =  builder.Configuration["Auth0:ClientId"];
                options.ClientSecret = builder.Configuration["Auth0:ClientSecret"];
        
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
            
                
                options.CallbackPath = new PathString("/callback");
                options.ClaimsIssuer = "Auth0";
                options.SaveTokens = true;
        
                options.Events.OnTokenValidated = context => Task.CompletedTask;
        
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name"
                };
              
                
                options.Events = new OpenIdConnectEvents
                {
                    OnRedirectToIdentityProvider = context =>
                    {
                        Console.WriteLine(context.ProtocolMessage.RedirectUri);
                        context.ProtocolMessage.RedirectUri = context.ProtocolMessage.RedirectUri.Replace("http://", "https://", StringComparison.OrdinalIgnoreCase);
                        Console.WriteLine(context.ProtocolMessage.RedirectUri);
                        return Task.FromResult(0);
                    },
                    OnRedirectToIdentityProviderForSignOut = context =>
                    {
                        var logoutUri =
                            $"https://{builder.Configuration["Auth0:Domain"]}/v2/logout?client_id={builder.Configuration["Auth0:ClientId"]}";
        
                        var postLogoutUri = context.Properties.RedirectUri;
                        if (!string.IsNullOrEmpty(postLogoutUri))
                        {
                            if (postLogoutUri.StartsWith("/"))
                            {
                                var request = context.Request;
                                postLogoutUri = request.Scheme + "://" + request.Host + request.PathBase +
                                                postLogoutUri;
                            }
        
                            logoutUri += $"&returnTo={Uri.EscapeDataString(postLogoutUri)}";
                        }
        
                        context.Response.Redirect(logoutUri);
                        context.HandleResponse();
        
                        return Task.CompletedTask;
                    }
                };
            });
        
        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.None;
        });
         
         
        //     builder.Services.AddAuth0WebAppAuthentication(options =>
        //     {
        //         options.Domain = $"https://{builder.Configuration["Auth0:Domain"]}";
        //         options.ClientId = builder.Configuration["Auth0:ClientId"];
        //         options.Scope = "openid profile";
        //     });
    }
    



    public static void AddLogger(this WebApplicationBuilder builder)
    {
        var loggerConfig = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.WithExceptionDetails()
            .Destructure.UsingAttributes()
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("ApplicationName", "Robot-API")
            .Enrich.With(new RemovePropertiesEnricher())
            .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
            .MinimumLevel.Override("System", LogEventLevel.Error)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss}] [{SourceContext}] [{Level}] {Message}{NewLine}{Exception}");
   

        var logger = loggerConfig.CreateLogger();
        SelfLog.Enable(Console.Error);
        builder.Host.UseSerilog(logger);
        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(logger);
    }
}