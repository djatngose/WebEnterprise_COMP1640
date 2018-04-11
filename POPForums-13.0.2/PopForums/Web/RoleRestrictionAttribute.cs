using System.Web.Mvc;
using PopForums.Models;

namespace PopForums.Web
{
	public abstract class RoleRestrictionAttribute : AuthorizeAttribute
	{
		protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
		{
			var result = new ViewResult { ViewName = "Forbidden" };
			filterContext.Result = result;
		}

		protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
		{
		    var user =(User) httpContext.User;

            return httpContext.User != null && (user.Email=="admin@gmail.com" || user.Email=="manager@gmail.com");
		}

		protected abstract string RoleToRestrictTo { get; }
	}
}
