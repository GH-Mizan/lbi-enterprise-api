using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using LbI.AccountHeads.Dto;
using LbI.Customers.Dto;
using LbI.Entities;
using LbI.Enums;
using LbI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LbI.AccountHeads
{
    public class AccountHeadAppService : LbIAppServiceBase, IAccountHeadAppService 
    {
        private readonly IRepository<AccountHead> _accountHeadRepository;
        public AccountHeadAppService(IRepository<AccountHead> accountHeadRepository)
        {
            _accountHeadRepository = accountHeadRepository;
        }

        public async Task<List<AccountHeadOutputDto>> GetAccountHeadsAsync(AccountHeadType? type)
        {
            return (await _accountHeadRepository.GetAllListAsync(x => type == null || x.Type == type))
                .Select(s => new AccountHeadOutputDto()
                {
                    Id = s.Id,
                    Name = s.Name,
                    Type = s.Type,
                    TypeText = s.Type.DisplayName(),
                    ParentHead = s.ParentHead,
                    ParentName = s.ParentHead.DisplayName()
                }).ToList();
        }

        public async Task<AccountHeadEntryDto> GetAsync(int id)
        {
            var entity = await _accountHeadRepository.GetAsync(id);
            return new AccountHeadEntryDto() { Id = entity.Id, Name = entity.Name, Type = entity.Type, ParentHead = entity.ParentHead };
        }

        public async Task CreateOrUpdateAsync(AccountHeadEntryDto input)
        {
            if (input.Id.HasValue)
            {
                var ah = await _accountHeadRepository.GetAsync(input.Id.Value);
                ah.Name = input.Name;
                ah.Type = input.Type;
                ah.ParentHead = input.ParentHead;
                await _accountHeadRepository.UpdateAsync(ah);
            }
            else
            {
                var entity = new AccountHead() { ParentHead = input.ParentHead, Name = input.Name, Type = input.Type };
                await _accountHeadRepository.InsertAsync(entity);
            }
        }

        public async Task AccountHeadRemoveAsync(int id)
        {
            var customer = await _accountHeadRepository.GetAsync(id);
            await _accountHeadRepository.DeleteAsync(customer);
        }

        public async Task<List<ComboboxItemDto>> GetAccountHeadSelectListAsync(AccountHeadType type)
        {
            var heads = await _accountHeadRepository.GetAllListAsync(x => x.Type == type);
            return heads.Select(s => new ComboboxItemDto()
            {
                Value = s.Id.ToString(),
                DisplayText = $"{s.Name} ({s.ParentHead.DisplayName()}) "
            }).OrderBy(o => o.DisplayText).ToList();
        }
        public List<ComboboxItemDto> GetAccountHeadTypeSelectListAsync()
        {
            var output = ((AccountHeadType[])Enum.GetValues(typeof(AccountHeadType))).Select(c => new ComboboxItemDto() { Value = ((int)c).ToString(), DisplayText = c.DisplayName() }).ToList();
            return output;
        }

        public List<ComboboxItemDto> GetParentAccountHeadsSelectListAsync()
        {
            var output = ((ParentAccountHead[])Enum.GetValues(typeof(ParentAccountHead))).Select(c => new ComboboxItemDto() { Value = ((int)c).ToString(), DisplayText = c.DisplayName() }).ToList();
            return output;
        }
    }
}
