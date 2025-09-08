using Abp.Application.Services;
using Abp.Application.Services.Dto;
using LbI.Designations.Dto;
using System.Threading.Tasks;

namespace LbI.Designations
{
    public interface IDesignationAppService : IApplicationService
    {
        Task<PagedResultDto<DesignationOutputDto>> GetPaginatedDesignationsAsync(DesignationsFilterDto filter);
        Task<DesignationCreateOrUpdateDto> GetAsync(int id);
        Task CreateOrUpdateAsync(DesignationCreateOrUpdateDto input);
        Task DeleteAsync(int id);
    }
}
