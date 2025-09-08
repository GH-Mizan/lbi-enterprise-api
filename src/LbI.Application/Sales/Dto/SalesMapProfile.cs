using AutoMapper;
using LbI.Entities;

namespace LbI.Sales.Dto
{
    public class SalesMapProfile : Profile
    {
        public SalesMapProfile()
        {
            CreateMap<Sale, SalesOutputDto>();
            CreateMap<SalesEntryDto, Sale>();
            CreateMap<Sale, SalesEntryDto>();
            CreateMap<SalesDetailsEntryDto, SaleDetail>();
            CreateMap<SaleDetail, SalesDetailsEntryDto>();
            CreateMap<DueReceivedHistory, DueReceivedHistoryDto>();
            CreateMap<DueReceivedHistoryDto, DueReceivedHistory>();
        }
    }
}
