using AutoMapper;
using LbI.Entities;
using System.Text.RegularExpressions;

namespace LbI.Departments.Dto
{
    public class DepartmentMapProfile: Profile
    {
        public DepartmentMapProfile()
        {
            CreateMap<DepartmentCreateOrUpdateDto, Department>();
            CreateMap<Department, DepartmentOutputDto>();
            CreateMap<Department, DepartmentCreateOrUpdateDto>();
        }
    }
}
