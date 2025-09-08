using Abp.Application.Services;
using Abp.Application.Services.Dto;
using LbI.Purchases.Dto;
using System.Threading.Tasks;

namespace LbI.Purchases
{
    public interface IPurchaseAppService : IApplicationService
    {
        Task<PagedResultDto<PurchaseOutputDto>> GetPaginatedPurchasesAsync(PurchasesFilterDto filter);
    }
}
