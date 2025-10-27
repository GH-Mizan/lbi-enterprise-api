using Abp.Domain.Repositories;
using LbI.Entities;
using LbI.Products.Dto;
using LbI.Purchases.Dto;
using LbI.VirtualItems.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LbI.VirtualItems
{
    public class VirtualItemAppService: LbIAppServiceBase, IVirtualItemAppService
    {
        private readonly IRepository<VirtualItem> _virtualItemRepo;
        public VirtualItemAppService(IRepository<VirtualItem> virtualItemRepo)
        {
            _virtualItemRepo = virtualItemRepo;
        }

        public async Task<List<VirtualItemOutput>> GetAllAsync()
        {
            var items = await _virtualItemRepo.GetAllListAsync(x=> x.ActiveStatus);
            return ObjectMapper.Map<List<VirtualItemOutput>>(items);
        }

        public async Task<VirtualItemEntryInput> GetAsync(int id)
        {
            var entity = await _virtualItemRepo.GetAsync(id);
            return ObjectMapper.Map<VirtualItemEntryInput>(entity);
        }

        public async Task CreateOrUpdateAsync(VirtualItemEntryInput input)
        {
            if (input.Id.HasValue)
            {
                var vi = await _virtualItemRepo.GetAsync(input.Id.Value);
                ObjectMapper.Map(input, vi);
                await _virtualItemRepo.UpdateAsync(vi);
            }
            else
            {
                var vi = ObjectMapper.Map<VirtualItem>(input);
                await _virtualItemRepo.InsertAsync(vi);
            }
        }

        
    }
}
