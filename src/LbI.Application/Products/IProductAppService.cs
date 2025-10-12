using Abp.Application.Services;
using Abp.Application.Services.Dto;
using LbI.Products.Dto;
using System.Threading.Tasks;

namespace LbI.Products
{
    public interface IProductAppService : IApplicationService
    {
        Task<PagedResultDto<ProductOutputDto>> GetPaginatedProductsAsync(ProductsFilterDto filter);
        Task<ProductCreateOrUpdateDto> GetAsync(int id);
        Task CreateOrUpdateAsync(ProductCreateOrUpdateDto input);
        Task ProductRemoveAsync(int id);
    }
}
