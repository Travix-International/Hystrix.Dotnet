using System.Collections.Generic;

namespace Hystrix.Dotnet
{
    public class HystrixOptions
    {
        public HystrixOptions()
        {
            ConfigurationServiceImplementation = "HystrixLocalConfigurationService";
            LocalOptions = HystrixLocalOptions.CreateDefault();
        }

        public static HystrixOptions CreateDefault()
        {
            return new HystrixOptions();
        }

        public string ConfigurationServiceImplementation { get; set; }

        public HystrixJsonConfigurationSourceOptions  JsonConfigurationSourceOptions { get; set; }

        public HystrixLocalOptions LocalOptions { get; set; }
    }
}