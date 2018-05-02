using Microsoft.Graph;
using System.Threading.Tasks;

namespace AspNetCore.MSGraphAPI.Services
{
    public interface IGraphAuthProvider
    {
        Task<string> GetUserAccessTokenAsync(string userId, string[] scopes);
        GraphServiceClient GetAuthenticatedClient(string userId, string[] scopes);
        GraphServiceClient GetAuthenticatedClient(string accessToken);
    }
}
