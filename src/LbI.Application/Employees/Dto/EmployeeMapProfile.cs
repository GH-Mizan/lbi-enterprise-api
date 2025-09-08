using AutoMapper;
using LbI.Entities;

namespace LbI.Employees.Dto
{
    public class EmployeeMapProfile: Profile
    {
        public EmployeeMapProfile()
        {
            CreateMap<EmployeeCreateOrUpdateDto, Employee>();
            CreateMap<Employee, EmployeeOutputDto>();
            CreateMap<Employee, EmployeeCreateOrUpdateDto>();
        }
    }
}
