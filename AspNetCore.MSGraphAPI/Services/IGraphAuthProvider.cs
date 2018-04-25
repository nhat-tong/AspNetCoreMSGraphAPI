using System.Threading.Tasks;

namespace AspNetCore.MSGraphAPI.Services
{
    public interface IGraphAuthProvider
    {
        Task<string> GetUserAccessTokenAsync(string userId);
    }
}
