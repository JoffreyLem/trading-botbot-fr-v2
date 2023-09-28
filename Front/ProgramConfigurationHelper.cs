using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Syncfusion.Blazor;
using Syncfusion.Licensing;

namespace Front;

public static class ProgramConfigurationHelper
{

    public static void AddSyncFusion(this WebApplicationBuilder builder)
    {
        SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NHaF5cXmtCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdgWXZceHVURmFfVkNwWEI=");
        builder.Services.AddSyncfusionBlazor();
    }
    
    public static void AddAuthentification(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddOpenIdConnect("Auth0", options => {
                options.Authority = $"https://{builder.Configuration["Auth0:Domain"]}";

                options.ClientId = builder.Configuration["Auth0:ClientId"];
                options.ClientSecret = builder.Configuration["Auth0:ClientSecret"];

                options.ResponseType = OpenIdConnectResponseType.Code;

                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");

                options.CallbackPath = new PathString("/callback");
                options.ClaimsIssuer = "Auth0";
                options.SaveTokens = true;
                
                options.Events.OnTokenValidated = context =>
                {
                    var claims = context.Principal.Claims;
                    return Task.CompletedTask;
                };
                
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name"
                };

                // Add handling of lo
                options.Events = new OpenIdConnectEvents
                {
                    OnRedirectToIdentityProviderForSignOut = (context) =>
                    {
                        var logoutUri = $"https://{builder.Configuration["Auth0:Domain"]}/v2/logout?client_id={builder.Configuration["Auth0:ClientId"]}";

                        var postLogoutUri = context.Properties.RedirectUri;
                        if (!string.IsNullOrEmpty(postLogoutUri))
                        {
                            if (postLogoutUri.StartsWith("/"))
                            {
                                var request = context.Request;
                                postLogoutUri = request.Scheme + "://" + request.Host + request.PathBase + postLogoutUri;
                            }
                            logoutUri += $"&returnTo={ Uri.EscapeDataString(postLogoutUri)}";
                        }

                        context.Response.Redirect(logoutUri);
                        context.HandleResponse();

                        return Task.CompletedTask;
                    }
                };
            });
    }
}