using Abp.Application.Services;
using LbI.MultiTenancy.Dto;

namespace LbI.MultiTenancy;

public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>
{
}

