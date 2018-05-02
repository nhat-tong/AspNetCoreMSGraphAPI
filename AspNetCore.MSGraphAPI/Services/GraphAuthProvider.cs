#region using
using AspNetCore.MSGraphAPI.Framework;
using AspNetCore.MSGraphAPI.Framework.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
#endregion

namespace AspNetCore.MSGraphAPI.Services
{
    public class GraphAuthProvider : IGraphAuthProvider
    {
        private readonly IMemoryCache _memoryCache;
        private readonly AzureADOptions _azureOptions;
        private TokenCache _userTokenCache;

        public GraphAuthProvider(IMemoryCache memoryCache, IOptions<AzureADOptions> options)
        {
            _memoryCache = memoryCache;
            _azureOptions = options.Value;
        }

        public async Task<string> GetUserAccessTokenAsync(string userId, string[] scopes)
        {
            _userTokenCache = new MemoryTokenCache(userId, _memoryCache).GetCacheInstance();

            var cca = new ConfidentialClientApplication(
                _azureOptions.ClientId,
                _azureOptions.BaseUrl + _azureOptions.CallbackPath,
                new ClientCredential(_azureOptions.ClientSecret),
                _userTokenCache,
                null);

            var result = await cca.AcquireTokenSilentAsync(scopes, cca.Users.FirstOrDefault());
            return result.AccessToken;
        }

        public GraphServiceClient GetAuthenticatedClient(string userId, string[] scopes)
        {
            return new GraphServiceClient(new DelegateAuthenticationProvider(
                async request =>
                {
                    // Passing tenant ID to the sample auth provider to use as a cache key
                    var accessToken = await GetUserAccessTokenAsync(userId, scopes);

                    // Append the access token to the request
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                }));
        }

        public GraphServiceClient GetAuthenticatedClient(string accessToken)
        {
            return new GraphServiceClient(new DelegateAuthenticationProvider(
                async request =>
                {
                    // Append the access token to the request
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    await Task.FromResult(0);
                }));
        }
    }
}
