using System.Net.Http.Headers;
using Microsoft.Graph;

namespace AspNetCore.MSGraphAPI.Services
{
    public class GraphSdkHelper : IGraphSdkHelper
    {
        private readonly IGraphAuthProvider _provider;

        public GraphSdkHelper(IGraphAuthProvider provider)
        {
            _provider = provider;
        }

        public GraphServiceClient GetAuthenticatedClient(string userId)
        {
            return new GraphServiceClient(new DelegateAuthenticationProvider(
                async request =>
                {
                    // Passing tenant ID to the sample auth provider to use as a cache key
                    var accessToken = await _provider.GetUserAccessTokenAsync(userId);

                    // Append the access token to the request
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                }));
        }
    }
}
