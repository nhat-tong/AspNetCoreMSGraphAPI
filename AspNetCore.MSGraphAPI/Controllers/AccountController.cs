using AspNetCore.MSGraphAPI.Framework;
using AspNetCore.MSGraphAPI.Framework.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace AspNetCore.MSGraphAPI.Controllers
{
    public class AccountController : Controller
    {
        private AzureADOptions _azureOptions;

        public AccountController(IOptions<AzureADOptions> options)
        {
            _azureOptions = options.Value;
        }

        [HttpGet]
        public IActionResult SignIn()
        {
            return Challenge(new AuthenticationProperties { RedirectUri = "/" });
        }

        [HttpGet]
        public IActionResult SignOut()
        {
            return SignOut(CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public ActionResult PermissionsRequired(string error)
        {
            ViewBag.Error = error;
            return View();
        }

        [Authorize]
        public ActionResult ConnectAADTenant()
        {
            // Redirect the admin to grant your app permissions
            string tenantId = User.FindFirstValue(Constants.TenantIdType);
            string url = string.Format(_azureOptions.AdminConsentFormat, tenantId, _azureOptions.ClientId, "whatever_you_want", _azureOptions.BaseUrl + "/Account/AADTenantConnected");
            return new RedirectResult(url);
        }

        // When the admin completes granting the permissions, they will be redirected here.
        [Authorize]
        public void AADTenantConnected(string state, string tenant, string admin_consent, string error, string error_description)
        {
            if (error != null)
            {
                // If the admin did not grant permissions, ask them to do so again
                Response.Redirect("/Account/PermissionsRequired?error=" + error_description);
                return;
            }

            // Note: Here the state parameter will contain whatever you passed in the outgoing request. You can
            // use this state to encode any information that you wish to track during execution of this request.

            Response.Redirect("/Group");
        }
    }
}