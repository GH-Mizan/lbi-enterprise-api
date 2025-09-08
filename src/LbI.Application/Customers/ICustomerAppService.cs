using Abp.Application.Services;
using Abp.Application.Services.Dto;
using LbI.Customers.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LbI.Customers
{
    public interface ICustomerAppService: IApplicationService
    {
        Task<PagedResultDto<CustomerOutputDto>> GetPaginatedCustomersAsync(CustomersFilterDto filter);
        Task<CustomerCreateOrUpdateDto> GetAsync(int id);
        Task CreateOrUpdateAsync(CustomerCreateOrUpdateDto input);
        Task DeleteAsync(int id);
    }
}
