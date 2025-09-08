using Abp.Application.Services;
using LbI.Sessions.Dto;
using System.Threading.Tasks;

namespace LbI.Sessions;

public interface ISessionAppService : IApplicationService
{
    Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
}
