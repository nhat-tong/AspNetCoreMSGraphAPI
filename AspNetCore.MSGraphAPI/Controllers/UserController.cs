using System.Security.Claims;
using System.Threading.Tasks;
using AspNetCore.MSGraphAPI.Framework;
using AspNetCore.MSGraphAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.MSGraphAPI.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IGraphSdkHelper _graphHelper;
        private readonly IUserService _userService;

        public UserController(IUserService userService, IGraphSdkHelper graphHelper)
        {
            _graphHelper = graphHelper;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var graphClient = _graphHelper.GetAuthenticatedClient(User.FindFirstValue(Constants.ObjectIdentifierType));
            var users = await _userService.GetUsers(graphClient);

            return View(users);
        }
    }
}