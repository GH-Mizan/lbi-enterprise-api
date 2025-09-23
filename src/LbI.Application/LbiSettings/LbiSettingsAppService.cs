using Abp.Domain.Repositories;
using LbI.Entities;
using LbI.Helpers;
using LbI.LbiSettings.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LbI.LbiSettings
{
    public class LbiSettingsAppService : LbIAppServiceBase, ILbiSettingsAppService
    {
        private readonly IRepository<LbiSetting> _repository;
        public LbiSettingsAppService(IRepository<LbiSetting> repository)
        {
            _repository = repository;
        }

        public async Task<List<LbiSettingsOutputDto>> GetAllAsync()
        {
            return (await _repository.GetAllAsync()).Select(s => new LbiSettingsOutputDto()
            {
                Id = s.Id,
                Key = s.Key,
                Value = s.Value,
                KeyText = s.Key.DisplayName()
            }).ToList();
        }

        public async Task UpdateAsync(LbiSettingsUpdateDto dto)
        {
            var entity = await _repository.SingleAsync(x=> x.Key == dto.Key);
            entity.Value = dto.Value;
            await _repository.UpdateAsync(entity);
        }
    }
}
