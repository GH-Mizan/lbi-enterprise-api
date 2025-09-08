using Abp.Application.Services;
using Abp.Application.Services.Dto;
using LbI.Purchases.Dto;
using LbI.Sales.Dto;
using System.Threading.Tasks;

namespace LbI.Sales
{
    public interface ISalesAppService : IApplicationService
    {
        Task<PagedResultDto<SalesOutputDto>> GetPaginatedSalesAsync(SalesFilterDto filter);
    }
}
