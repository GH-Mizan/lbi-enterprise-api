using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using LbI.Designations.Dto;
using LbI.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LbI.Designations
{
    public class DesignationAppService : LbIAppServiceBase, IDesignationAppService
    {
        private readonly IRepository<Designation> _designationRepo;
        public DesignationAppService(IRepository<Designation> designationRepo)
        {
            _designationRepo = designationRepo;
        }

        public async Task<PagedResultDto<DesignationOutputDto>> GetPaginatedDesignationsAsync(DesignationsFilterDto filter)
        {
            var searchText = string.IsNullOrEmpty(filter.SearchText) ? null : filter.SearchText.ToLower();
            var query = (from v in await _designationRepo.GetAllAsync()
                         select new DesignationOutputDto()
                         {
                             Id = v.Id,
                             Title = v.Title,
                             ActiveStatus = v.ActiveStatus
                         }).AsQueryable();

            if (searchText != null)
            {
                query = query.Where(x =>
                x.Title.ToLower().Contains(searchText));
            }

            var designations = query.OrderBy(o => o.Title).Skip(filter.Skip).Take(filter.Take).ToList();

            return new PagedResultDto<DesignationOutputDto>()
            {
                Items = designations,
                TotalCount = query.Count()
            };
        }

        public async Task<DesignationCreateOrUpdateDto> GetAsync(int id)
        {
            var entity = await _designationRepo.GetAsync(id);
            return ObjectMapper.Map<DesignationCreateOrUpdateDto>(entity);
        }

        public async Task CreateOrUpdateAsync(DesignationCreateOrUpdateDto input)
        {
            if (input.Id.HasValue)
            {
                var cuatomer = await _designationRepo.GetAsync(input.Id.Value);
                ObjectMapper.Map(input, cuatomer);
                await _designationRepo.UpdateAsync(cuatomer);
            }
            else
            {
                var cuatomer = ObjectMapper.Map<Designation>(input);
                await _designationRepo.InsertAsync(cuatomer);
            }
        }

        public async Task DesignationRemoveAsync(int id)
        {
            var designation = await _designationRepo.GetAsync(id);
            await _designationRepo.DeleteAsync(designation);
        }
    }
}
