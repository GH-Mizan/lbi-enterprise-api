using Abp.Application.Services.Dto;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.EntityFrameworkCore.Repositories;
using LbI.Customers.Dto;
using LbI.DailyCashes.Dto;
using LbI.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LbI.DailyCashes
{
    public class DailyCashAppService : LbIAppServiceBase, IDailyCashAppService
    {
        private readonly IRepository<DailyCash> _dailyCashRepo;
        public DailyCashAppService(
            IRepository<DailyCash> dailyCashRepo)
        {
            _dailyCashRepo = dailyCashRepo;
        }

        public async Task<PagedResultDto<DailyCashOutputDto>> GetPaginatedDailyCashAsync(DailyCashFilterDto filter)
        {
            var query = await _dailyCashRepo.GetAllAsync();

            if(filter.StartDate.HasValue && filter.EndDate.HasValue)
            {

            }
            var data = await query.OrderByDescending(o=> o.Date).Skip(filter.Skip).Take(filter.Take).ToListAsync();
            
            return new PagedResultDto<DailyCashOutputDto>()
            {
                Items = ObjectMapper.Map<List<DailyCashOutputDto>>(data),
                TotalCount = query.Count()
            };
        }

        public async Task<CreateOrUpdateDailyCashInput> GetAsync(int id)
        {
            var entity = await _dailyCashRepo.GetAsync(id);
            return new CreateOrUpdateDailyCashInput()
            {
                DailyCashInfo = ObjectMapper.Map<DailyCashEntryDto>(entity)
            };
        }

        public async Task<CreateOrUpdateDailyCashInput> GetByDateAsync(DateTime date)
        {
            var output = new CreateOrUpdateDailyCashInput();
            if (date.Day == 8 && date.Day == 8 && date.Year == 2025)
            {
                var entity = await _dailyCashRepo.FirstOrDefaultAsync(x => x.Date.Day == date.Day && x.Date.Month == date.Month && x.Date.Year == date.Year);
                if(entity != null)
                {
                    output.DailyCashInfo = ObjectMapper.Map<DailyCashEntryDto>(entity);
                }
            }
            else
            {
                var expectedCash = await _dailyCashRepo.FirstOrDefaultAsync(x => x.Date.Day == date.Day && x.Date.Month == date.Month && x.Date.Year == date.Year);
                if(expectedCash != null)
                {
                    output.PrevCashInfo = null;
                    output.DailyCashInfo = ObjectMapper.Map<DailyCashEntryDto>(expectedCash);
                }
                else
                {
                    var prevCash = await _dailyCashRepo.FirstOrDefaultAsync(x => x.Date.Day == date.Day - 1 && x.Date.Month == date.Month && x.Date.Year == date.Year);
                    output.PrevCashInfo = ObjectMapper.Map<DailyCashEntryDto>(prevCash);
                }
            }
            return output;
            
        }
        public async Task<bool> CheckByDate(DateTime date)
        {
            var count = await _dailyCashRepo.CountAsync(x => x.Date.Day == date.Day);
            return count > 0;
        }

        public async Task<DateTime?> GetLastEntryDateAsync()
        {
            var lastDate = (await _dailyCashRepo.GetAllAsync()).OrderByDescending(x => x.Date).FirstOrDefault()?.Date;
            return lastDate;
        }

        public async Task<int> CreateOrUpdateDailyCashAsync( CreateOrUpdateDailyCashInput input)
        {
            var id = input.DailyCashInfo.Id;
            if (id.HasValue)
            {
                var dailyCash = await _dailyCashRepo.GetAsync(id.Value);
                ObjectMapper.Map(input.DailyCashInfo, dailyCash);
                await _dailyCashRepo.UpdateAsync(dailyCash);
                return id.Value;
            }
            else
            {
                var dailyCash = ObjectMapper.Map<DailyCash>(input.DailyCashInfo);
                return await _dailyCashRepo.InsertAndGetIdAsync(dailyCash);
            }
        }
    }
}
