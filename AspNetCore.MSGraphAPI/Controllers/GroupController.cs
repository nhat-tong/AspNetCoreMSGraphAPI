#region using
using AspNetCore.MSGraphAPI.Framework.Extensions;
using AspNetCore.MSGraphAPI.Models;
using AspNetCore.MSGraphAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace AspNetCore.MSGraphAPI.Controllers
{
    public class GroupController : BaseController
    {
        private readonly IGraphAuthProvider _graphAuthProvider;
        private AzureADOptions _azureOptions;
        private string[] _adminRestrictedGroupReadScopes = new string[] { "group.read.all" };
        private string[] _adminRestrictedGroupReadWriteScopes = new string[] { "Group.ReadWrite.All" };

        #region Constructor
        public GroupController(IGraphAuthProvider graphAuthProvider, IOptions<AzureADOptions> options)
        {
            _graphAuthProvider = graphAuthProvider;
            _azureOptions = options.Value;
        }
        #endregion

        #region Group
        public async Task<IActionResult> Index()
        {
            GraphServiceClient client = null;
            string accessToken = string.Empty;
            try
            {
                // Try to get a token for our basic set of scopes
                accessToken = await _graphAuthProvider.GetUserAccessTokenAsync(UserId, _azureOptions.GraphScopes.Split(' '));
            }
            catch (MsalException)
            {
                return Redirect("/Account/SignIn");
            }

            try
            {
                // Get a token for our admin-restricted set of scopes Microsoft Graph
                accessToken = await _graphAuthProvider.GetUserAccessTokenAsync(UserId, _adminRestrictedGroupReadScopes);
                client = _graphAuthProvider.GetAuthenticatedClient(accessToken);
            }
            catch (MsalException)
            {
                return Redirect("/Account/PermissionsRequired");
            }

            var groups = await client.Groups.Request().GetAsync();
            return View(groups);
        }

        [HttpGet]
        public IActionResult CreateGroup()
        {
            return PartialView("_FormCreationGroup", new CreateGroupViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> CreateGroup(CreateGroupViewModel model)
        {
            GraphServiceClient client = _graphAuthProvider.GetAuthenticatedClient(UserId, _adminRestrictedGroupReadWriteScopes);
            await client.Groups.Request().AddAsync(new Group
            {
                DisplayName = model.GroupName,
                Description = model.GroupDescription,
                SecurityEnabled = true,
                MailEnabled = false,
                MailNickname = model.GroupName
            });

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> DeleteGroup(string id)
        {
            GraphServiceClient client = _graphAuthProvider.GetAuthenticatedClient(UserId, _adminRestrictedGroupReadWriteScopes);
            await client.Groups[id].Request().DeleteAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Detail(string id)
        {
            GraphServiceClient client = _graphAuthProvider.GetAuthenticatedClient(UserId, _adminRestrictedGroupReadWriteScopes);
            // Ref: Odata maximum navigation ($expand) is 1 https://github.com/microsoftgraph/microsoft-graph-docs/issues/96
            Group group = await client.Groups[id].Request().Expand("members").GetAsync();
            group.Owners = await client.Groups[id].Owners.Request().GetAsync();

            return View(group);
        }
        #endregion

        #region Member
        [HttpGet]
        public async Task<IActionResult> CreateMember(string id)
        {
            GraphServiceClient client = _graphAuthProvider.GetAuthenticatedClient(UserId, _adminRestrictedGroupReadWriteScopes);
            IGraphServiceUsersCollectionPage users = await client.Users.Request().GetAsync();
            IGroupMembersCollectionWithReferencesPage members = await client.Groups[id].Members.Request().GetAsync();

            var excludedMembers = members.CurrentPage.Select(m => m.Id);
            var proposalMembers = users.CurrentPage.Where(u => !excludedMembers.Contains(u.Id));

            return PartialView("_FormCreationMemberOrOwner", new CreateMemberOrOwnerViewModel
            {
                ActionName = "CreateMember",
                GroupId = id,
                ListItem = proposalMembers.Select(m => new SelectListItem { Text = m.UserPrincipalName, Value = m.Id })
            });
        }

        /// <summary>
        /// Add member to group
        /// </summary>
        /// <param name="model"></param>
        ///<see cref="https://stackoverflow.com/questions/33959752/write-requests-are-only-supported-on-contained-entities-microsoft-graph-api"/>
        ///<seealso cref="https://developer.microsoft.com/en-us/graph/docs/api-reference/v1.0/api/group_post_members"/>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateMember(CreateMemberOrOwnerViewModel model)
        {
            GraphServiceClient client = _graphAuthProvider.GetAuthenticatedClient(UserId, _adminRestrictedGroupReadWriteScopes);
            // Get the request URL
            var requestUrl = client.Groups[model.GroupId].Members.Request().RequestUrl + "/$ref";
            foreach (var member in model.SelectedItems)
            {
                // Payload in json
                string body = "{\"@odata.id\":\"https://graph.microsoft.com/v1.0/users/" + member + "\"}";

                // Create the request message and add the content.
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");

                // Authenticate (add access token) our HttpRequestMessage
                await client.AuthenticationProvider.AuthenticateRequestAsync(request);

                // Send the request and get the response.
                await client.HttpProvider.SendAsync(request);
            }

            return RedirectToAction("Detail", new { id = model.GroupId });
        }

        [HttpGet]
        public async Task<IActionResult> RevokeMember(string id, string idGroup)
        {
            GraphServiceClient client = _graphAuthProvider.GetAuthenticatedClient(UserId, _adminRestrictedGroupReadWriteScopes);
            var requestUrl = client.Groups[idGroup].Members[id].Request().RequestUrl + "/$ref";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, requestUrl);
            await client.AuthenticationProvider.AuthenticateRequestAsync(request);
            await client.HttpProvider.SendAsync(request);

            return RedirectToAction("Detail", new { id = idGroup });
        }
        #endregion

        #region Owner
        [HttpGet]
        public async Task<IActionResult> CreateOwner(string id)
        {
            GraphServiceClient client = _graphAuthProvider.GetAuthenticatedClient(UserId, _adminRestrictedGroupReadWriteScopes);
            IGraphServiceUsersCollectionPage users = await client.Users.Request().GetAsync();
            IGroupOwnersCollectionWithReferencesPage owners = await client.Groups[id].Owners.Request().GetAsync();

            var excludedOwners = owners.CurrentPage.Select(m => m.Id);
            var proposalOwners = users.CurrentPage.Where(u => !excludedOwners.Contains(u.Id));

            return PartialView("_FormCreationMemberOrOwner", new CreateMemberOrOwnerViewModel
            {
                ActionName = "CreateOwner",
                GroupId = id,
                ListItem = proposalOwners.Select(m => new SelectListItem { Text = m.UserPrincipalName, Value = m.Id })
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateOwner(CreateMemberOrOwnerViewModel model)
        {
            GraphServiceClient client = _graphAuthProvider.GetAuthenticatedClient(UserId, _adminRestrictedGroupReadWriteScopes);
            // Get the request URL
            var requestUrl = client.Groups[model.GroupId].Owners.Request().RequestUrl + "/$ref";
            foreach (var owner in model.SelectedItems)
            {
                // Payload in json
                string body = "{\"@odata.id\":\"https://graph.microsoft.com/v1.0/users/" + owner + "\"}";

                // Create the request message and add the content.
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");

                // Authenticate (add access token) our HttpRequestMessage
                await client.AuthenticationProvider.AuthenticateRequestAsync(request);

                // Send the request and get the response.
                await client.HttpProvider.SendAsync(request);
            }

            return RedirectToAction("Detail", new { id = model.GroupId });
        }

        [HttpGet]
        public async Task<IActionResult> RevokeOwner(string id, string idGroup)
        {
            GraphServiceClient client = _graphAuthProvider.GetAuthenticatedClient(UserId, _adminRestrictedGroupReadWriteScopes);
            var requestUrl = client.Groups[idGroup].Owners[id].Request().RequestUrl + "/$ref";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, requestUrl);
            await client.AuthenticationProvider.AuthenticateRequestAsync(request);
            await client.HttpProvider.SendAsync(request);

            return RedirectToAction("Detail", new { id = idGroup });
        }
        #endregion
    }
}