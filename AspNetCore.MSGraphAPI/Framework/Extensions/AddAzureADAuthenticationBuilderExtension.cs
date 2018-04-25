#region using
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
#endregion

namespace AspNetCore.MSGraphAPI.Framework.Extensions
{
    public static class AddAzureADAuthenticationBuilderExtension
    {
        public static AuthenticationBuilder AddAzureAD(this AuthenticationBuilder builder, Action<AzureADOptions> options)
        {
            builder.Services.Configure(options);
            builder.Services.AddSingleton<IConfigureOptions<OpenIdConnectOptions>, ConfigureAzureOptions>();

            builder.AddOpenIdConnect();

            return builder;
        }
    }

    public class ConfigureAzureOptions : IConfigureNamedOptions<OpenIdConnectOptions>
    {
        private readonly AzureADOptions _azureOptions;

        public ConfigureAzureOptions(IOptions<AzureADOptions> options)
        {
            _azureOptions = options.Value;
        }

        public void Configure(string name, OpenIdConnectOptions options)
        {
            options.ClientId = _azureOptions.ClientId;
            options.Authority = _azureOptions.Authority;
            options.UseTokenLifetime = false;
            options.CallbackPath = _azureOptions.CallbackPath;
            options.RequireHttpsMetadata = false;
            options.ResponseType = OpenIdConnectResponseType.CodeIdToken;
            var allScopes = $"{_azureOptions.Scopes} {_azureOptions.GraphScopes}".Split(new[] { ' ' });
            foreach (var scope in allScopes) { options.Scope.Add(scope); }

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false
            };

            options.Events = new OpenIdConnectEvents
            {
                OnTicketReceived = context =>
                {
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    context.Response.Redirect("/Home/Error");
                    context.HandleResponse();
                    return Task.CompletedTask;
                },
                OnAuthorizationCodeReceived = async context =>
                {
                    var authorizationCode = context.ProtocolMessage.Code;
                    var identifier = context.Principal.FindFirstValue(Constants.ObjectIdentifierType);
                    var memoryCache = context.HttpContext.RequestServices.GetService<IMemoryCache>();
                    var graphScopes = _azureOptions.GraphScopes.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    var cca = new ConfidentialClientApplication(
                        _azureOptions.ClientId,
                        _azureOptions.BaseUrl + _azureOptions.CallbackPath,
                        new ClientCredential(_azureOptions.ClientSecret),
                        new MemoryTokenCache(identifier, memoryCache).GetCacheInstance(),
                        null);

                    var result = await cca.AcquireTokenByAuthorizationCodeAsync(authorizationCode, graphScopes);
                    context.HandleCodeRedemption(result.AccessToken, result.IdToken);
                }
            };
        }

        public void Configure(OpenIdConnectOptions options)
        {
            Configure(Options.DefaultName, options);
        }
    }
}
