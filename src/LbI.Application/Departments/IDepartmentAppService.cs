using Abp.Application.Services;
using Abp.Application.Services.Dto;
using LbI.Departments.Dto;
using System.Threading.Tasks;

namespace LbI.Departments
{
    public interface IDepartmentAppService : IApplicationService
    {
        Task<PagedResultDto<DepartmentOutputDto>> GetPaginatedDepartmentsAsync(DepartmentsFilterDto filter);
        Task<DepartmentCreateOrUpdateDto> GetAsync(int id);
        Task CreateOrUpdateAsync(DepartmentCreateOrUpdateDto input);
        Task DeleteAsync(int id);
    }
}
