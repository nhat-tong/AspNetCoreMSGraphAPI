using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.MSGraphAPI.Controllers
{
    public abstract class BaseController : Controller
    {
        protected string UserId { get { return User?.FindFirstValue(Framework.Constants.ObjectIdentifierType); } }
        protected string TenantId { get { return User?.FindFirstValue(Framework.Constants.TenantIdType); } }
    }
}