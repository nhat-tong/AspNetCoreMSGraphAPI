using Microsoft.Graph;

namespace AspNetCore.MSGraphAPI.Services
{
    public interface IGraphSdkHelper
    {
        GraphServiceClient GetAuthenticatedClient(string userId);
    }
}
