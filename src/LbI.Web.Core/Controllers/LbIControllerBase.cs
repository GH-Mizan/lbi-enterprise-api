using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Identity;

namespace LbI.Controllers
{
    public abstract class LbIControllerBase : AbpController
    {
        protected LbIControllerBase()
        {
            LocalizationSourceName = LbIConsts.LocalizationSourceName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
