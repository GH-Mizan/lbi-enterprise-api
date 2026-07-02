using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using LbI.AdditionalParties.Dto;
using LbI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LbI.AdditionalParties
{
    public class AdditionalPartiesAppService : LbIAppServiceBase, IAdditionalPartiesAppService
    {
        private readonly IRepository<AdditionalPartiesAdvance> _additionalPartiesRepo;
        public AdditionalPartiesAppService(IRepository<AdditionalPartiesAdvance> additionalPartiesRepo)
        {
            _additionalPartiesRepo = additionalPartiesRepo;
        }

        public async Task<PagedResultDto<AdditionalPartyOutputDto>> GetPaginatedAdditionalPartiesAdvancesAsync(AdditionalPartiesFilterDto filter)
        {
            var searchText = string.IsNullOrEmpty(filter.SearchText) ? null : filter.SearchText.ToLower();
            var query = (from v in await _additionalPartiesRepo.GetAllAsync()
                         select new AdditionalPartyOutputDto()
                         {
                             Id = v.Id,
                             PartyName = v.PartyName,
                             ContactNumber = v.ContactNumber,
                             Email = v.Email,
                             Address = v.Address,
                             Relation = v.Relation,
                             Advance = v.Advance,
                             Notes = v.Notes
                         }).AsQueryable();

            if (searchText != null)
            {
                query = query.Where(x =>
                x.PartyName.ToLower().Contains(searchText)
                || x.Email.ToLower().Contains(searchText)
                || x.ContactNumber.ToLower().Contains(searchText)
                || x.Address.ToLower().Contains(searchText)
                );
            }

            var aps = query.OrderByDescending(o => o.Id).Skip(filter.Skip).Take(filter.Take).ToList();

            return new PagedResultDto<AdditionalPartyOutputDto>()
            {
                Items = aps,
                TotalCount = query.Count()
            };
        }

        public async Task<AdditionalPartyEntryDto> GetAsync(int id)
        {
            var entity = await _additionalPartiesRepo.GetAsync(id);
            return ObjectMapper.Map<AdditionalPartyEntryDto>(entity);
        }

        public async Task CreateOrUpdateAsync(AdditionalPartyEntryDto input)
        {
            if (input.Id.HasValue)
            {
                var ap = await _additionalPartiesRepo.GetAsync(input.Id.Value);
                ObjectMapper.Map(input, ap);
                await _additionalPartiesRepo.UpdateAsync(ap);
            }
            else
            {
                var ap = ObjectMapper.Map<AdditionalPartiesAdvance>(input);
                await _additionalPartiesRepo.InsertAsync(ap);
            }
        }

        public async Task AdditionalPartyRemoveAsync(int id)
        {
            var department = await _additionalPartiesRepo.GetAsync(id);
            await _additionalPartiesRepo.DeleteAsync(department);
        }

        public async Task<List<ComboboxItemDto>> GetAdditionalPartiesSelectlistAsync()
        {
            return (await _additionalPartiesRepo.GetAllListAsync()).Select(s => new ComboboxItemDto
            {
                Value = s.Id.ToString(),
                DisplayText = s.PartyName.ToString()
            }).ToList();
        }
    }
}
