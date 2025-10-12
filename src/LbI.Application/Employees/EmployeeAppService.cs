using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using LbI.Entities;
using LbI.Employees.Dto;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LbI.Employees
{
    public class EmployeeAppService : LbIAppServiceBase, IEmployeeAppService
    {
        private readonly IRepository<Employee> _employeeRepo;
        private readonly IRepository<Department> _departmentRepo;
        private readonly IRepository<Designation> _designationRepo;
        public EmployeeAppService(
            IRepository<Employee> employeeRepo,
            IRepository<Department> departmentRepo,
            IRepository<Designation> designationRepo
            )
        {
            _employeeRepo = employeeRepo;
            _departmentRepo = departmentRepo;
            _designationRepo = designationRepo;
        }

        public async Task<PagedResultDto<EmployeeOutputDto>> GetPaginatedEmployeesAsync(EmployeesFilterDto filter)
        {
            var searchText = string.IsNullOrEmpty(filter.SearchText) ? null : filter.SearchText.ToLower();
            var query = (from e in await _employeeRepo.GetAllAsync()
                         join dp in await _departmentRepo.GetAllAsync() on e.DepartmentId equals dp.Id
                         join dg in await _designationRepo.GetAllAsync() on e.DesignationId equals dg.Id
                         select new EmployeeOutputDto()
                         {
                             Id = e.Id,
                             Name = e.Name,
                             DepartmentId = e.DepartmentId,
                             DepartmentName =  dp.Name,
                             DesignationId = e.DesignationId,
                             DesignationName = dg.Title,
                             JoiningDate = e.JoiningDate,
                             BirthDate = e.BirthDate,
                             BloodGroup = e.BloodGroup,
                             ContactNumber = e.ContactNumber,
                             Address = e.Address,
                             Reference = e.Reference,
                             Remarks = e.Remarks,
                             ActiveStatus = e.ActiveStatus
                         }).AsQueryable();

            if (searchText != null)
            {
                query = query.Where(x =>
                x.Name.ToLower().Contains(searchText) ||
                x.DepartmentName.ToLower().Contains(searchText) ||
                x.DesignationName.ToLower().Contains(searchText) ||
                x.ContactNumber.ToLower().Contains(searchText) ||
                x.Address.ToLower().Contains(searchText) ||
                x.Remarks.ToLower().Contains(searchText));
            }

            var employees = query.OrderBy(o => o.Name).Skip(filter.Skip).Take(filter.Take).ToList();

            return new PagedResultDto<EmployeeOutputDto>()
            {
                Items = employees,
                TotalCount = query.Count()
            };
        }

        public async Task<EmployeeCreateOrUpdateDto> GetAsync(int id)
        {
            var entity = await _employeeRepo.GetAsync(id);
            return ObjectMapper.Map<EmployeeCreateOrUpdateDto>(entity);
        }

        public async Task CreateOrUpdateAsync(EmployeeCreateOrUpdateDto input)
        {
            if (input.Id.HasValue)
            {
                var cuatomer = await _employeeRepo.GetAsync(input.Id.Value);
                ObjectMapper.Map(input, cuatomer);
                await _employeeRepo.UpdateAsync(cuatomer);
            }
            else
            {
                var cuatomer = ObjectMapper.Map<Employee>(input);
                await _employeeRepo.InsertAsync(cuatomer);
            }
        }

        public async Task EmployeeRemoveAsync(int id)
        {
            var employee = await _employeeRepo.GetAsync(id);
            await _employeeRepo.DeleteAsync(employee);
        }

        public async Task<List<ComboboxItemDto>> GetDepartmentsAsync()
        {
           return (await _departmentRepo.GetAllListAsync(x=> x.ActiveStatus)).Select(s=> new ComboboxItemDto()
           {
               Value = s.Id.ToString(),
               DisplayText = s.Name
           }).ToList();
        }

        public async Task<List<ComboboxItemDto>> GetDesignationsAsync()
        {
            return (await _designationRepo.GetAllListAsync(x => x.ActiveStatus)).Select(s => new ComboboxItemDto()
            {
                Value = s.Id.ToString(),
                DisplayText = s.Title
            }).ToList();
        }

        public async Task<List<ComboboxItemDto>> GetEmployeesAsync()
        {
            return (await _employeeRepo.GetAllListAsync(x => x.ActiveStatus)).Select(s => new ComboboxItemDto()
            {
                Value = s.Id.ToString(),
                DisplayText = s.Name
            }).ToList();
        }
    }
}
