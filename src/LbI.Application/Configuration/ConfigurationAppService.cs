using Abp.Authorization;
using Abp.Runtime.Session;
using LbI.Configuration.Dto;
using System.Threading.Tasks;

namespace LbI.Configuration;

[AbpAuthorize]
public class ConfigurationAppService : LbIAppServiceBase, IConfigurationAppService
{
    public async Task ChangeUiTheme(ChangeUiThemeInput input)
    {
        await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettingNames.UiTheme, input.Theme);
    }
}
