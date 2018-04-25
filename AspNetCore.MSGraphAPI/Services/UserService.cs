using System.Threading.Tasks;
using Microsoft.Graph;

namespace AspNetCore.MSGraphAPI.Services
{
    public class UserService : IUserService
    {
        public async Task<IGraphServiceUsersCollectionPage> GetUsers(GraphServiceClient client)
        {
            return await client.Users.Request().GetAsync();
        }
    }
}
