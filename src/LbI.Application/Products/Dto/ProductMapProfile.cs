using AutoMapper;
using LbI.Entities;
using System.Text.RegularExpressions;

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
