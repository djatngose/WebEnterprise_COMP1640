using System.Web.Mvc;
using PopForums.Models;

namespace PopForums.Web
{
	public abstract class RoleRestrictionAttributeNew : AuthorizeAttribute
	{
		protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
		{
			var result = new ViewResult { ViewName = "Forbidden" };
			filterContext.Result = result;
		}

		protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
		{
		    var user =(User) httpContext.User;

            return httpContext.User != null;
		}

		protected abstract string RoleToRestrictTo { get; }
	}
}
