using Abp.Application.Services;
using Abp.Application.Services.Dto;
using LbI.Suppliers.Dto;
using System.Threading.Tasks;

namespace LbI.Suppliers
{
    public interface ISupplierAppService : IApplicationService
    {
        Task<PagedResultDto<SupplierOutputDto>> GetPaginatedSupplierssAsync(SuppliersFilterDto filter);
        Task<SupplierCreateOrUpdateDto> GetAsync(int id);
        Task CreateOrUpdateAsync(SupplierCreateOrUpdateDto input);
        Task DeleteAsync(int id);
    }
}
