using Abp.Authorization;
using LbI.Authorization.Roles;
using LbI.Authorization.Users;

namespace LbI.Authorization;

public class PermissionChecker : PermissionChecker<Role, User>
{
    public PermissionChecker(UserManager userManager)
        : base(userManager)
    {
    }
}
