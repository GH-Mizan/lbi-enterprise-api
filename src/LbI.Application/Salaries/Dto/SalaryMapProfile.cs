using AutoMapper;
using LbI.Entities;

namespace LbI.Salaries.Dto
{
    public class SalaryMapProfile : Profile
    {
        public SalaryMapProfile()
        {
            CreateMap<SalaryAdvanceDto, SalaryAdvance>();
            CreateMap<SalaryAdvance, SalaryAdvanceDto>();

            CreateMap<SalaryAdvanceDto, SalaryAdvanceHistory>();
            CreateMap<SalaryAdvanceHistory, SalaryAdvanceDto>();

            CreateMap<SalaryEntryInputDto, Salary>();
            CreateMap<Salary, SalaryEntryInputDto>();
        }
    }
}
