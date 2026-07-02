using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using LbI.Entities;
using LbI.Departments.Dto;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LbI.Departments
{
    public class DepartmentAppService : LbIAppServiceBase, IDepartmentAppService
    {
        private readonly IRepository<Department> _departmentRepo;
        public DepartmentAppService(IRepository<Department> departmentRepo)
        {
            _departmentRepo = departmentRepo;
        }

        public async Task<PagedResultDto<DepartmentOutputDto>> GetPaginatedDepartmentsAsync(DepartmentsFilterDto filter)
        {
            var searchText = string.IsNullOrEmpty(filter.SearchText) ? null : filter.SearchText.ToLower();
            var query = (from v in await _departmentRepo.GetAllAsync()
                         select new DepartmentOutputDto()
                         {
                             Id = v.Id,
                             Name = v.Name,
                             ActiveStatus = v.ActiveStatus
                         }).AsQueryable();

            if (searchText != null)
            {
                query = query.Where(x =>
                x.Name.ToLower().Contains(searchText));
            }

            var departments = query.OrderBy(o => o.Name).Skip(filter.Skip).Take(filter.Take).ToList();

            return new PagedResultDto<DepartmentOutputDto>()
            {
                Items = departments,
                TotalCount = query.Count()
            };
        }

        public async Task<DepartmentCreateOrUpdateDto> GetAsync(int id)
        {
            var entity = await _departmentRepo.GetAsync(id);
            return ObjectMapper.Map<DepartmentCreateOrUpdateDto>(entity);
        }

        public async Task CreateOrUpdateAsync(DepartmentCreateOrUpdateDto input)
        {
            if (input.Id.HasValue)
            {
                var department = await _departmentRepo.GetAsync(input.Id.Value);
                ObjectMapper.Map(input, department);
                await _departmentRepo.UpdateAsync(department);
            }
            else
            {
                var department = ObjectMapper.Map<Department>(input);
                await _departmentRepo.InsertAsync(department);
            }
        }

        public async Task DepartmentRemoveAsync(int id)
        {
            var department = await _departmentRepo.GetAsync(id);
            await _departmentRepo.DeleteAsync(department);
        }
    }
}
