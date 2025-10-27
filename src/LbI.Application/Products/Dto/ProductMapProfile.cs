using AutoMapper;
using LbI.Entities;

namespace LbI.Products.Dto
{
    public class ProductMapProfile: Profile
    {
        public ProductMapProfile()
        {
            CreateMap<ProductCreateOrUpdateDto, Product>();
            CreateMap<Product, ProductOutputDto>();
            CreateMap<Product, ProductCreateOrUpdateDto>();
        }
    }
}
