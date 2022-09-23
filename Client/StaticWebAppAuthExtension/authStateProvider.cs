using BlazorApp.Client.StaticWebAppAuthExtension.Model;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Net.Http.Json;
using System.Security.Claims;

namespace BlazorApp.Client.StaticWebAppAuthExtension
{

    public static class StaticWebAppsAuthenticationExtensions
    {
        public static IServiceCollection AddStaticWebAppsAuthentication(this IServiceCollection services)
        {
            return services
                .AddAuthorizationCore()
                .AddScoped<AuthenticationStateProvider, StaticWebAppsAuthenticationStateProvider>();
        }
    }

    public class StaticWebAppsAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IConfiguration config;
        private readonly HttpClient http;


        public StaticWebAppsAuthenticationStateProvider(IConfiguration config, IWebAssemblyHostEnvironment environment)
        {
            this.config = config;
            this.http = new HttpClient { BaseAddress = new Uri(environment.BaseAddress) };
        }

        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var authDataUrl = config.GetValue<string>("StaticWebAppsAuthentication:AuthenticationDataUrl", "/.auth/me");
                var data = await http.GetFromJsonAsync<AuthenticationData>(authDataUrl);

                var principal = data!.ClientPrincipal;
                if (principal is not null )
                {
                    if (principal.UserRoles is not null)
                        principal.UserRoles = principal.UserRoles.Except(new string[] { "anonymous" }, StringComparer.CurrentCultureIgnoreCase);

                    var identity = new ClaimsIdentity(principal.IdentityProvider);
                    if (principal.UserDetails is not null) identity.AddClaim(new Claim(ClaimTypes.Name, principal.UserDetails));
                    if (principal.UserRoles is not null) identity.AddClaims(principal.UserRoles.Select(r => new Claim(ClaimTypes.Role, r)));
                    if (principal.Claims is not null)
                    {
                        foreach (var claim in principal.Claims.Where(c => c.Typ != "" && c.Val != ""))
                            identity.AddClaim(new Claim(claim.Typ, claim.Val));
                    }

                    return new AuthenticationState(new ClaimsPrincipal(identity));

                }

                return new AuthenticationState(new ClaimsPrincipal());    
            }
            catch
            {
                return new AuthenticationState(new ClaimsPrincipal());
            }
        }
    }
}
