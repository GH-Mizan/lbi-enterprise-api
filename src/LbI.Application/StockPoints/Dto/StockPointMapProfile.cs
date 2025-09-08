using AutoMapper;
using LbI.Entities;

namespace LbI.StockPoints.Dto
{
    public class StockPointMapProfile: Profile
    {
        public StockPointMapProfile()
        {
            CreateMap<StockPointCreateOrUpdateDto, StockPoint>();
            CreateMap<StockPoint, StockPointOutputDto>();
            CreateMap<StockPoint, StockPointCreateOrUpdateDto>();
        }
    }
}
