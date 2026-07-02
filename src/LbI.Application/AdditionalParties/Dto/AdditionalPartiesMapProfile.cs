using AutoMapper;
using LbI.Departments.Dto;
using LbI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LbI.AdditionalParties.Dto
{
    public class AdditionalPartiesMapProfile : Profile
    {
        public AdditionalPartiesMapProfile()
        {
            CreateMap<AdditionalPartyEntryDto, AdditionalPartiesAdvance>();
            CreateMap<AdditionalPartiesAdvance, AdditionalPartyEntryDto>();
        }
       
    }
}
