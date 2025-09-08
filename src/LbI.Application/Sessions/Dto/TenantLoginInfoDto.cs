using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using LbI.MultiTenancy;

namespace LbI.Sessions.Dto;

[AutoMapFrom(typeof(Tenant))]
public class TenantLoginInfoDto : EntityDto
{
    public string TenancyName { get; set; }

    public string Name { get; set; }
}
