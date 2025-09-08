using Abp.Application.Services;
using LbI.Authorization.Accounts.Dto;
using System.Threading.Tasks;

namespace LbI.Authorization.Accounts;

public interface IAccountAppService : IApplicationService
{
    Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input);

    Task<RegisterOutput> Register(RegisterInput input);
}
