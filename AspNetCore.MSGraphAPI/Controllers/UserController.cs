#region using
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.MSGraphAPI.Framework.Extensions;
using AspNetCore.MSGraphAPI.Models;
using AspNetCore.MSGraphAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Identity.Client;
#endregion

namespace AspNetCore.MSGraphAPI.Controllers
{
    [Authorize]
    public class UserController : BaseController
    {
        private readonly IGraphAuthProvider _graphAuthProvider;
        private readonly string[] _userConsentScopes = new[] { "User.ReadWrite.All" };
        private AzureADOptions _azureOptions { get; }

        public UserController(IGraphAuthProvider graphAuthProvider, IOptions<AzureADOptions> options)
        {
            _graphAuthProvider = graphAuthProvider;
            _azureOptions = options.Value;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var graphClient =  _graphAuthProvider.GetAuthenticatedClient(UserId, _azureOptions.GraphScopes.Split(' '));
                var users = await graphClient.Users.Request().GetAsync();

                ViewBag.TenantId = TenantId;
                return View(users);
            }
            catch(MsalException)
            {
                // access token invalid or expired, user must sign-in again
                return Redirect("/Account/SignIn");
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateUser()
        {
            GraphServiceClient graphClient = _graphAuthProvider.GetAuthenticatedClient(UserId, _userConsentScopes);
            // gets the tenant domain from the Organization object to construct the user's email address.
            IGraphServiceOrganizationCollectionPage organization = await graphClient.Organization.Request().GetAsync();
            string domain = organization.CurrentPage[0].VerifiedDomains.ElementAt(0).Name;

            return PartialView("_FormCreationUser", new CreateUserViewModel { Domain = domain });
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            GraphServiceClient client = _graphAuthProvider.GetAuthenticatedClient(UserId, _userConsentScopes);
            var user = await client.Users.Request().AddAsync(new User
            {
                AccountEnabled = true,
                UserPrincipalName = string.Concat(model.UserPrincipalName, "@", model.Domain),
                MailNickname = model.UserPrincipalName,
                GivenName = model.FirstName,
                Surname = model.LastName,
                DisplayName = model.DisplayName,
                JobTitle = model.JobTitle,
                Department = model.Department,
                PasswordProfile = new PasswordProfile
                {
                    ForceChangePasswordNextSignIn = false,
                    Password = model.Password
                }
            });

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> DeleteUser(string id)
        {
            GraphServiceClient client = _graphAuthProvider.GetAuthenticatedClient(UserId, _userConsentScopes);
            var me = await client.Me.Request().Select("id").GetAsync();
            if(me.Id != id)
            {
                await client.Users[id].Request().DeleteAsync();
            }

            return RedirectToAction("Index");
        }
    }
}