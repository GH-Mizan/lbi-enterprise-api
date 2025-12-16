using Abp.Domain.Repositories;
using LbI.Employees.Dto;
using LbI.Entities;
using LbI.SalesOrders.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace LbI.SalesOrders
{
    public class SalesOrderAppService : LbIAppServiceBase, ISalesOrderAppService
    {
        private readonly IRepository<SalesOrder> _salesOrderRepository;
        private readonly IRepository<Customer> _customerRepository;
        public SalesOrderAppService(
            IRepository<SalesOrder> salesOrderRepository,
            IRepository<Customer> customerRepository
            )
        {
            _salesOrderRepository = salesOrderRepository;
            _customerRepository = customerRepository;
        }

        public async Task<List<SalesOrderOutputDto>> GetOrdersAsync(DateTime date)
        {
            return (from so in await _salesOrderRepository.GetAllAsync()
                    join c in await _customerRepository.GetAllAsync() on so.CustomerId equals c.Id
                    where so.Date.Date == date.Date
                    select new SalesOrderOutputDto()
                    {
                        Id = so.Id,
                        CustomerId = so.CustomerId,
                        CustomerName = c.Name,
                        NitrousQty = so.NitrousQty,
                        AirQty = so.AirQty,
                        Oxygen136Qty = so.Oxygen136Qty,
                        Oxygen980Qty = so.Oxygen980Qty
                    }).OrderByDescending(o=> o.Oxygen980Qty).ThenByDescending(t=> t.AirQty).ThenByDescending(t => t.Oxygen136Qty).ThenByDescending(t => t.NitrousQty).ToList();
        }

        public async Task<SalesOrderCreateUpdateDto> GetAsync(int id)
        {
            var entity = await _salesOrderRepository.GetAsync(id);
            return ObjectMapper.Map<SalesOrderCreateUpdateDto>(entity);
        }

        public async Task CreateOrUpdateAsync(SalesOrderCreateUpdateDto input)
        {
            if (input.Id.HasValue)
            {
                var so = await _salesOrderRepository.GetAsync(input.Id.Value);
                ObjectMapper.Map(input, so);
                await _salesOrderRepository.UpdateAsync(so);
            }
            else
            {
                var so = ObjectMapper.Map<SalesOrder>(input);
                await _salesOrderRepository.InsertAsync(so);
            }
        }

        public async Task SalesOrderRemoveAsync(int id)
        {
            var so = await _salesOrderRepository.GetAsync(id);
            await _salesOrderRepository.DeleteAsync(so);
        }
    }
}
