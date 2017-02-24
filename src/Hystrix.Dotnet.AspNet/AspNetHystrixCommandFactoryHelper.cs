using System.Configuration;
using Microsoft.Extensions.Options;

namespace Hystrix.Dotnet.AspNet
{
    public class AspNetHystrixCommandFactoryHelper
    {
        public IHystrixCommandFactory CreateFactory()
        {
            var configSection = ConfigurationManager.GetSection("hystrix.dotnet/hystrix") as HystrixConfigSection;

            return CreateFactory(configSection);
        }

        public IHystrixCommandFactory CreateFactory(HystrixConfigSection configSection)
        {
            var translator = new HystrixConfigSectionTranslator();

            return new HystrixCommandFactory(
                Options.Create(configSection == null ? HystrixOptions.CreateDefault() : translator.TranslateToOptions(configSection)));
        }
    }
}