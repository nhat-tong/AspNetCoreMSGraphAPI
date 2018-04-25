#region using
using AspNetCore.MSGraphAPI.Framework;
using AspNetCore.MSGraphAPI.Framework.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System.Linq;
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

        public async Task<string> GetUserAccessTokenAsync(string userId)
        {
            _userTokenCache = new MemoryTokenCache(userId, _memoryCache).GetCacheInstance();

            var cca = new ConfidentialClientApplication(
                _azureOptions.ClientId,
                _azureOptions.BaseUrl + _azureOptions.CallbackPath,
                new ClientCredential(_azureOptions.ClientSecret),
                _userTokenCache,
                null);

            if (!cca.Users.Any()) throw new ServiceException(new Error
            {
                Code = "TokenNotFound",
                Message = "User not found in token cache. Maybe the server was restarted."
            });

            try
            {
                var result = await cca.AcquireTokenSilentAsync(_azureOptions.GraphScopes.Split(new[] { ' ' }), cca.Users.First());
                return result.AccessToken;
            }
            catch // Unable to retrieve the access token silently
            {
                throw new ServiceException(new Error
                {
                    Code = GraphErrorCode.AuthenticationFailure.ToString(),
                    Message = "Caller needs to authenticate. Unable to retrieve the access token silently."
                });
            }
        }
    }
}
