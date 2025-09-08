using AutoMapper;
using LbI.Entities;

namespace LbI.Purchases.Dto
{
    public class PurchaseMapProfile : Profile
    {
        public PurchaseMapProfile()
        {
            CreateMap<Purchase, PurchaseOutputDto>();
            CreateMap<PurchaseEntryDto, Purchase>();
            CreateMap<Purchase, PurchaseEntryDto>();
            CreateMap<PurchaseDetailsEntryDto, PurchaseDetail>();
            CreateMap<PurchaseDetail, PurchaseDetailsEntryDto>();
            CreateMap<DuePaymentHistory, DuePaymentHistoryDto>();
            CreateMap<DuePaymentHistoryDto, DuePaymentHistory>();
        }
    }
}
