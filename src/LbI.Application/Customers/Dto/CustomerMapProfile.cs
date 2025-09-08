using AutoMapper;
using LbI.Entities;

namespace LbI.Customers.Dto
{
    public class CustomerMapProfile: Profile
    {
        public CustomerMapProfile()
        {
            CreateMap<CustomerCreateOrUpdateDto, Customer>();
            CreateMap<Customer, CustomerOutputDto>();
            CreateMap<Customer, CustomerCreateOrUpdateDto>();
        }
    }
}
