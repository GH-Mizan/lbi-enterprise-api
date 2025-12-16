using AutoMapper;
using LbI.Entities;
using LbI.Purchases.Dto;

namespace LbI.SalesOrders.Dto
{
    public class SalesOrderMapProfile : Profile
    {
        public SalesOrderMapProfile()
        {
            CreateMap<SalesOrder, SalesOrderCreateUpdateDto>();
            CreateMap<SalesOrderCreateUpdateDto, SalesOrder>();
        }
    }
}
