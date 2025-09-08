using Abp.Application.Services;
using Abp.Application.Services.Dto;
using LbI.StockPoints.Dto;
using System.Threading.Tasks;

namespace LbI.StockPoints
{
    public interface IStockPointAppService : IApplicationService
    {
        Task<PagedResultDto<StockPointOutputDto>> GetPaginatedStockPointsAsync(StockPointsFilterDto filter);
        Task<StockPointCreateOrUpdateDto> GetAsync(int id);
        Task CreateOrUpdateAsync(StockPointCreateOrUpdateDto input);
        Task DeleteAsync(int id);
    }
}
