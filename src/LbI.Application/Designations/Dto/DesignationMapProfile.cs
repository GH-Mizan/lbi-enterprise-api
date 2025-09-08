using AutoMapper;
using LbI.Entities;

namespace LbI.Designations.Dto
{
    public class DesignationMapProfile: Profile
    {
        public DesignationMapProfile()
        {
            CreateMap<DesignationCreateOrUpdateDto, Designation>();
            CreateMap<Designation, DesignationOutputDto>();
            CreateMap<Designation, DesignationCreateOrUpdateDto>();
        }
    }
}
