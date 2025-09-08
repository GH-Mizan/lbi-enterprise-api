using AutoMapper;
using LbI.Entities;

namespace LbI.DailyCashes.Dto
{
    public class DailyCashMapProfile : Profile
    {
        public DailyCashMapProfile()
        {
            CreateMap<DailyCash, DailyCashOutputDto>();
            CreateMap<DailyCashEntryDto, DailyCash>();
            CreateMap<DailyCash, DailyCashEntryDto>();
        }
        
    }
}
