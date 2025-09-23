using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using LbI.Customers.Dto;
using LbI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LbI.Customers
{
    public class CustomerAppService : LbIAppServiceBase, ICustomerAppService
    {
        private readonly IRepository<Customer> _customerRepo;
        private readonly IRepository<CustomerPrice> _customerPriceRepo;
        private readonly IRepository<Product> _productRepo;
        public CustomerAppService(
            IRepository<Customer> customerRepo,
            IRepository<CustomerPrice> customerPriceRepo,
            IRepository<Product> productRepo
            )
        {
            _customerRepo = customerRepo;
            _customerPriceRepo = customerPriceRepo;
            _productRepo = productRepo;
        }

        public async Task<PagedResultDto<CustomerOutputDto>> GetPaginatedCustomersAsync(CustomersFilterDto filter)
        {
            var searchText = string.IsNullOrEmpty(filter.SearchText) ? null : filter.SearchText.ToLower();
            var query = (from c in await _customerRepo.GetAllAsync()
                         select new CustomerOutputDto()
                         {
                             Id = c.Id,
                             Name = c.Name,
                             ShortName = c.ShortName,
                             Address = c.Address,
                             ContactNo = c.ContactNo,
                             Email = c.Email,
                             ActiveStatus = c.ActiveStatus,
                             InitialDue = c.InitialDue,
                             Remarks = c.Remarks
                         }).AsQueryable();

            if (searchText != null)
            {
                query = query.Where(x =>
                x.Name.ToLower().Contains(searchText) ||
                x.ShortName.ToLower().Contains(searchText) ||
                x.Address.ToLower().Contains(searchText) ||
                x.ContactNo.ToLower().Contains(searchText) ||
                x.Email.ToLower().Contains(searchText));
            }

            var customers = query.OrderBy(o => o.Name).Skip(filter.Skip).Take(filter.Take).ToList();

            return new PagedResultDto<CustomerOutputDto>()
            {
                Items = customers,
                TotalCount = query.Count()
            };
        }

        public async Task<CustomerCreateOrUpdateDto> GetAsync(int id)
        {
            var entity = await _customerRepo.GetAsync(id);
            return ObjectMapper.Map<CustomerCreateOrUpdateDto>(entity);
        }

        public async Task CreateOrUpdateAsync(CustomerCreateOrUpdateDto input)
        {
            if (input.Id.HasValue)
            {
                var customer = await _customerRepo.GetAsync(input.Id.Value);
                ObjectMapper.Map(input, customer);
                await _customerRepo.UpdateAsync(customer);
            }
            else
            {
                var customer = ObjectMapper.Map<Customer>(input);
                await _customerRepo.InsertAsync(customer);
            }
        }

        public async Task DeleteAsync(int id)
        {
            var customer = await _customerRepo.GetAsync(id);
            await _customerRepo.DeleteAsync(customer);
        }

        public async Task<List<ComboboxItemDto>> GetCustomersSelectListAsync()
        {
            return (await _customerRepo.GetAllListAsync(x => x.ActiveStatus)).Select(s => new ComboboxItemDto()
            {
                Value = s.Id.ToString(),
                DisplayText = s.Name
            }).ToList();
        }

        public async Task<List<CustomerPriceDto>> GetCustomerPricesAsync(int customerId)
        {
            var products = await _productRepo.GetAllListAsync(x=> x.ActiveStatus);
            var customerPrices = await _customerPriceRepo.GetAllListAsync(x=> x.CustomerId == customerId);
            var output = new List<CustomerPriceDto>();
            foreach (var product in products) 
            {
                var customerPrice = new CustomerPriceDto()
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.SellPrice,
                    CustomerId = customerId,
                };
                var cp = customerPrices.FirstOrDefault(f=> f.ProductId == product.Id);
                if(cp != null)
                {
                    customerPrice.Id = cp.Id;
                    customerPrice.Price = cp.Price;
                }
                output.Add(customerPrice);
            }
            return output;
        }

        [UnitOfWork]
        public async Task SaveCustomerPricesAsync(List<CustomerPriceDto> input)
        {
            var customerId = input.First().CustomerId;
            var customerPrices = await _customerPriceRepo.GetAllListAsync(x => x.CustomerId == customerId);
            foreach (var cp in input) 
            {
                var customerPrice = customerPrices.FirstOrDefault(f => f.ProductId == cp.ProductId);
                if (customerPrice != null)
                {
                    customerPrice.Price = cp.Price;
                    await _customerPriceRepo.UpdateAsync(customerPrice);
                }
                else
                {
                    var newCP = new CustomerPrice()
                    {
                        CustomerId = customerId,
                        ProductId = cp.ProductId,
                        Price = cp.Price
                    };
                    await _customerPriceRepo.InsertAsync(newCP);
                }
            }
        }
    }
}
