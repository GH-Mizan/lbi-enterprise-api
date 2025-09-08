using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using LbI.Authorization;

namespace LbI;

[DependsOn(
    typeof(LbICoreModule),
    typeof(AbpAutoMapperModule))]
public class LbIApplicationModule : AbpModule
{
    public override void PreInitialize()
    {
        Configuration.Authorization.Providers.Add<LbIAuthorizationProvider>();
    }

    public override void Initialize()
    {
        var thisAssembly = typeof(LbIApplicationModule).GetAssembly();

        IocManager.RegisterAssemblyByConvention(thisAssembly);

        Configuration.Modules.AbpAutoMapper().Configurators.Add(
            // Scan the assembly for classes which inherit from AutoMapper.Profile
            cfg => cfg.AddMaps(thisAssembly)
        );
    }
}
