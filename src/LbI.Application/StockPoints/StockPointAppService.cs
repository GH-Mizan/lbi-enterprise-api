using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using LbI.Entities;
using LbI.Enums;
using LbI.Helpers;
using LbI.StockPoints.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LbI.StockPoints
{
    public class StockPointAppService : LbIAppServiceBase, IStockPointAppService
    {
        private readonly IRepository<StockPoint> _stockPointRepo;
        public StockPointAppService(IRepository<StockPoint> stockPointRepo)
        {
            _stockPointRepo = stockPointRepo;
        }

        public async Task<PagedResultDto<StockPointOutputDto>> GetPaginatedStockPointsAsync(StockPointsFilterDto filter)
        {
            var searchText = string.IsNullOrEmpty(filter.SearchText) ? null : filter.SearchText.ToLower();
            var query = (from v in await _stockPointRepo.GetAllAsync()
                         select new StockPointOutputDto()
                         {
                             Id = v.Id,
                             Name = v.Name,
                             StockPointType = v.StockPointType,
                             StockPointNumber = v.StockPointNumber,
                             GpsTrackerNo = v.GpsTrackerNo,
                             ActiveStatus = v.ActiveStatus,
                             Remarks = v.Remarks
                         }).AsQueryable();

            if (searchText != null)
            {
                query = query.Where(x =>
                x.Name.ToLower().Contains(searchText) ||
                x.StockPointNumber.ToLower().Contains(searchText) ||
                x.GpsTrackerNo.ToLower().Contains(searchText) ||
                x.Remarks.ToLower().Contains(searchText));
            }

            var StockPoints = query.OrderBy(o => o.Name).Skip(filter.Skip).Take(filter.Take).ToList();

            return new PagedResultDto<StockPointOutputDto>()
            {
                Items = StockPoints.Select((x, i) => { x.StockPointTypeText = x.StockPointType.DisplayName(); return x; }).ToList(),
                TotalCount = query.Count()
            };
        }

        public async Task<StockPointCreateOrUpdateDto> GetAsync(int id)
        {
            var entity = await _stockPointRepo.GetAsync(id);
            return ObjectMapper.Map<StockPointCreateOrUpdateDto>(entity);
        }

        public async Task CreateOrUpdateAsync(StockPointCreateOrUpdateDto input)
        {
            if (input.Id.HasValue)
            {
                var cuatomer = await _stockPointRepo.GetAsync(input.Id.Value);
                ObjectMapper.Map(input, cuatomer);
                await _stockPointRepo.UpdateAsync(cuatomer);
            }
            else
            {
                var cuatomer = ObjectMapper.Map<StockPoint>(input);
                await _stockPointRepo.InsertAsync(cuatomer);
            }
        }

        public async Task DeleteAsync(int id)
        {
            var StockPoint = await _stockPointRepo.GetAsync(id);
            await _stockPointRepo.DeleteAsync(StockPoint);
        }

        public async Task<List<ComboboxItemDto>> GetStockPointsAsync(bool vehiclesOnly)
        {
            return (await _stockPointRepo.GetAllListAsync(x => x.ActiveStatus && (vehiclesOnly == false || x.StockPointType == StockPointType.Vehicle))).Select(s => new ComboboxItemDto()
            {
                Value = s.Id.ToString(),
                DisplayText = s.Name
            }).ToList();
        }

        public List<ComboboxItemDto> GetStockPointTypesAsync()
        {
            var output = ((StockPointType[])Enum.GetValues(typeof(StockPointType))).Select(c => new ComboboxItemDto() { Value = ((int)c).ToString(), DisplayText = c.DisplayName() }).ToList();
            return output;
        }
    }
}
