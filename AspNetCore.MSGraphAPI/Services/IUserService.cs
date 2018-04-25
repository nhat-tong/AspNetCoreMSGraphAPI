using Microsoft.Graph;
using System.Threading.Tasks;

namespace AspNetCore.MSGraphAPI.Services
{
    public interface IUserService
    {
        Task<IGraphServiceUsersCollectionPage> GetUsers(GraphServiceClient client);
    }
}
