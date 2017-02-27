using System.Configuration;

namespace Hystrix.Dotnet.AspNet
{
    public class AspNetHystrixCommandFactoryHelper
    {
        public IHystrixCommandFactory CreateFactory()
        {
            var configSection = ConfigurationManager.GetSection("hystrix.dotnet/hystrix") as HystrixConfigSection;

            var translator = new HystrixConfigSectionTranslator();

            return new HystrixCommandFactory(
                configSection == null ? HystrixOptions.CreateDefault() : translator.TranslateToOptions(configSection));
        }
    }
}