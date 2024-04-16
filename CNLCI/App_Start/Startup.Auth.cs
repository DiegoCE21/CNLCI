/*using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Configuration;
using System.Security.Claims;

namespace CNLCI
{
    public partial class Startup
    {
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string aadInstance = EnsureTrailingSlash(ConfigurationManager.AppSettings["ida:AADInstance"]);
        private static string tenantId = ConfigurationManager.AppSettings["ida:TenantId"];
        private static string postLogoutRedirectUri = ConfigurationManager.AppSettings["ida:PostLogoutRedirectUri"];
        private static string authority = aadInstance + tenantId + "/v2.0";

        public void ConfigureAuth(IAppBuilder app)
        {

            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {

                ExpireTimeSpan = TimeSpan.FromMinutes(1),

                SlidingExpiration = true,

                LoginPath = new PathString("/Login/Login"),

                CookieSameSite = SameSiteMode.None
            });

            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    ClientId = clientId,
                    Authority = authority,
                    PostLogoutRedirectUri = postLogoutRedirectUri,

                    Notifications = new OpenIdConnectAuthenticationNotifications()
                    {
                        SecurityTokenValidated = (context) =>
                        {
                            string name = context.AuthenticationTicket.Identity.FindFirst("name").Value;
                            context.AuthenticationTicket.Identity.AddClaim(new Claim(ClaimTypes.Name, name, string.Empty));
                            return System.Threading.Tasks.Task.FromResult(0);
                        }
                    }
                });


        }

        private static string EnsureTrailingSlash(string value)
        {
            if (value == null)
            {
                value = string.Empty;
            }

            if (!value.EndsWith("/", StringComparison.Ordinal))
            {
                return value + "/";
            }

            return value;
        }
    }
}*/


/*
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Configuration;

namespace CNLCI
{
    public partial class Startup
    {
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string aadInstance = EnsureTrailingSlash(ConfigurationManager.AppSettings["ida:AADInstance"]);
        private static string tenantId = ConfigurationManager.AppSettings["ida:TenantId"];
        private static string postLogoutRedirectUri = ConfigurationManager.AppSettings["ida:PostLogoutRedirectUri"];
        private static string authority = aadInstance + tenantId + "/v2.0";

        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = CookieAuthenticationDefaults.AuthenticationType,
                ExpireTimeSpan = TimeSpan.FromMinutes(1),
                SlidingExpiration = true,
                LoginPath = new PathString("/Login/Login"),
                CookieSameSite = SameSiteMode.None,
                CookieSecure = CookieSecureOption.Always 
            });

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                ClientId = clientId,
                Authority = authority,
                PostLogoutRedirectUri = postLogoutRedirectUri,
                RedirectUri = postLogoutRedirectUri,
                ResponseType = "id_token",
                Scope = "openid profile",
                TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true // False solo si hay problemas de validación y no se necesita validar el emisor.
                },
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    SecurityTokenValidated = context =>
                    {
                        var nameClaim = context.AuthenticationTicket.Identity.FindFirst("name");
                        if (nameClaim != null)
                        {
                            context.AuthenticationTicket.Identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, nameClaim.Value));
                        }
                        return System.Threading.Tasks.Task.FromResult(0);
                    },
                    AuthenticationFailed = context =>
                    {
                        context.HandleResponse();
                        context.Response.Redirect("/Error?message=" + context.Exception.Message);
                        return System.Threading.Tasks.Task.FromResult(0);
                    }
                }
            });
        }

        private static string EnsureTrailingSlash(string value)
        {
            if (!value.EndsWith("/", StringComparison.Ordinal))
            {
                return value + "/";
            }
            return value;
        }
    }
}

*/