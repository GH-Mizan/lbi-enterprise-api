using Abp.Events.Bus;
using Abp.Modules;
using Abp.Reflection.Extensions;
using LbI.Configuration;
using LbI.EntityFrameworkCore;
using LbI.Migrator.DependencyInjection;
using Castle.MicroKernel.Registration;
using Microsoft.Extensions.Configuration;

namespace LbI.Migrator;

[DependsOn(typeof(LbIEntityFrameworkModule))]
public class LbIMigratorModule : AbpModule
{
    private readonly IConfigurationRoot _appConfiguration;

    public LbIMigratorModule(LbIEntityFrameworkModule abpProjectNameEntityFrameworkModule)
    {
        abpProjectNameEntityFrameworkModule.SkipDbSeed = true;

        _appConfiguration = AppConfigurations.Get(
            typeof(LbIMigratorModule).GetAssembly().GetDirectoryPathOrNull()
        );
    }

    public override void PreInitialize()
    {
        Configuration.DefaultNameOrConnectionString = _appConfiguration.GetConnectionString(
            LbIConsts.ConnectionStringName
        );

        Configuration.BackgroundJobs.IsJobExecutionEnabled = false;
        Configuration.ReplaceService(
            typeof(IEventBus),
            () => IocManager.IocContainer.Register(
                Component.For<IEventBus>().Instance(NullEventBus.Instance)
            )
        );
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(LbIMigratorModule).GetAssembly());
        ServiceCollectionRegistrar.Register(IocManager);
    }
}
