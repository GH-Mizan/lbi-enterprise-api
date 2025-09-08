using Abp.AspNetCore;
using Abp.AspNetCore.TestBase;
using Abp.Modules;
using Abp.Reflection.Extensions;
using LbI.EntityFrameworkCore;
using LbI.Web.Startup;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace LbI.Web.Tests;

[DependsOn(
    typeof(LbIWebMvcModule),
    typeof(AbpAspNetCoreTestBaseModule)
)]
public class LbIWebTestModule : AbpModule
{
    public LbIWebTestModule(LbIEntityFrameworkModule abpProjectNameEntityFrameworkModule)
    {
        abpProjectNameEntityFrameworkModule.SkipDbContextRegistration = true;
    }

    public override void PreInitialize()
    {
        Configuration.UnitOfWork.IsTransactional = false; //EF Core InMemory DB does not support transactions.
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(LbIWebTestModule).GetAssembly());
    }

    public override void PostInitialize()
    {
        IocManager.Resolve<ApplicationPartManager>()
            .AddApplicationPartsIfNotAddedBefore(typeof(LbIWebMvcModule).Assembly);
    }
}