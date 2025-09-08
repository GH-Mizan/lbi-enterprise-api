using Abp.Modules;
using Abp.Reflection.Extensions;
using LbI.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace LbI.Web.Host.Startup
{
    [DependsOn(
       typeof(LbIWebCoreModule))]
    public class LbIWebHostModule : AbpModule
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public LbIWebHostModule(IWebHostEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(LbIWebHostModule).GetAssembly());
        }
    }
}
