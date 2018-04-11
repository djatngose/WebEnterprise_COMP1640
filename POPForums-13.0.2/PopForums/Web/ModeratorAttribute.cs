using PopForums.Models;

namespace PopForums.Web
{
	public class ModeratorAttribute : RoleRestrictionAttributeNew
	{
		protected override string RoleToRestrictTo
		{
			get { return PermanentRoles.Moderator; }
		}
	}
}
