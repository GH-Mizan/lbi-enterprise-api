using AutoMapper;
using LbI.Entities;
using LbI.Products.Dto;

namespace LbI.VirtualItems.Dto
{
    public class VirtualItemMapProfile : Profile
    {
        public VirtualItemMapProfile()
        {
            CreateMap<VirtualItemEntryInput, VirtualItem>();
            CreateMap<VirtualItem, VirtualItemEntryInput>();
            CreateMap<VirtualItem, VirtualItemOutput>();
        }
    }
}
