using LbI.Configuration.Dto;
using System.Threading.Tasks;

namespace LbI.Configuration;

public interface IConfigurationAppService
{
    Task ChangeUiTheme(ChangeUiThemeInput input);
}
