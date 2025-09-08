using AutoMapper;
using LbI.Entities;
using System.Text.RegularExpressions;

namespace LbI.Suppliers.Dto
{
    public class SupplierMapProfile: Profile
    {
        public SupplierMapProfile()
        {
            CreateMap<SupplierCreateOrUpdateDto, Supplier>();
            CreateMap<Supplier, SupplierOutputDto>();
            CreateMap<Supplier, SupplierCreateOrUpdateDto>();
        }
    }
}
