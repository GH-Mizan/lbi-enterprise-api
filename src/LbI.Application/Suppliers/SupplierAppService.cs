using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using LbI.Entities;
using LbI.Enums;
using LbI.Suppliers.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LbI.Suppliers
{
    public class SupplierAppService : LbIAppServiceBase, ISupplierAppService
    {
        private readonly IRepository<Supplier> _supplierRepo;
        public SupplierAppService(IRepository<Supplier> supplierRepo)
        {
            _supplierRepo = supplierRepo;
        }

        public async Task<PagedResultDto<SupplierOutputDto>> GetPaginatedSupplierssAsync(SuppliersFilterDto filter)
        {
            var searchText = string.IsNullOrEmpty(filter.SearchText) ? null : filter.SearchText.ToLower();
            var query = (from c in await _supplierRepo.GetAllAsync()
                         select new SupplierOutputDto()
                         {
                             Id = c.Id,
                             Name = c.Name,
                             ShortName = c.ShortName,
                             Address = c.Address,
                             ContactNo = c.ContactNo,
                             Email = c.Email,
                             ActiveStatus = c.ActiveStatus,
                             Remarks = c.Remarks
                         }).AsQueryable();

            if (searchText != null)
            {
                query = query.Where(x =>
                x.Name.ToLower().Contains(searchText) ||
                x.ShortName.ToLower().Contains(searchText) ||
                x.Address.ToLower().Contains(searchText) ||
                x.ContactNo.ToLower().Contains(searchText) ||
                x.Email.ToLower().Contains(searchText));
            }

            var suppliers = query.OrderBy(o => o.Name).Skip(filter.Skip).Take(filter.Take).ToList();

            return new PagedResultDto<SupplierOutputDto>()
            {
                Items = suppliers,
                TotalCount = query.Count()
            };
        }

        public async Task<SupplierCreateOrUpdateDto> GetAsync(int id)
        {
            var entity = await _supplierRepo.GetAsync(id);
            return ObjectMapper.Map<SupplierCreateOrUpdateDto>(entity);
        }

        public async Task CreateOrUpdateAsync(SupplierCreateOrUpdateDto input)
        {
            if (input.Id.HasValue)
            {
                var supplier = await _supplierRepo.GetAsync(input.Id.Value);
                ObjectMapper.Map(input, supplier);
                await _supplierRepo.UpdateAsync(supplier);
            }
            else
            {
                var supplier = ObjectMapper.Map<Supplier>(input);
                await _supplierRepo.InsertAsync(supplier);
            }
        }

        public async Task SupplierRemoveAsync(int id)
        {
            var supplier = await _supplierRepo.GetAsync(id);
            await _supplierRepo.DeleteAsync(supplier);
        }

        public async Task<List<ComboboxItemDto>> GetSuppliersSelectListAsync()
        {
            return (await _supplierRepo.GetAllListAsync(x => x.ActiveStatus)).Select(s => new ComboboxItemDto()
            {
                Value = s.Id.ToString(),
                DisplayText = s.Name
            }).ToList();
        }
    }
}
