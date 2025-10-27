using AutoMapper;
using LbI.Entities;
using LbI.Products.Dto;

namespace LbI.VirtualStocks.Dto
{
    public class VirtualStockMapProfile : Profile
    {
        public VirtualStockMapProfile()
        {
            CreateMap<VirtualStockEntryDto, VirtualStock>();
            CreateMap<VirtualStock, VirtualStockEntryDto>();
            CreateMap<VirtualStockDetailEntryDto, VirtualStockDetail>();
            CreateMap<VirtualStockDetail, VirtualStockDetailEntryDto>();
        }
    }
}
