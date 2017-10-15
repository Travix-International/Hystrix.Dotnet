using System.Configuration;

namespace Hystrix.Dotnet.WebConfiguration
{
    public class HystrixConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("serviceImplementation", DefaultValue = "HystrixLocalConfigurationService", IsRequired = false)]
        public string ServiceImplementation
        {
            get => this["serviceImplementation"].ToString();
            set => this["serviceImplementation"] = value;
        }

        [ConfigurationProperty("metricsStreamPollIntervalInMilliseconds", DefaultValue = "500", IsRequired = false)]
        public int MetricsStreamPollIntervalInMilliseconds
        {
            get => (int)this["metricsStreamPollIntervalInMilliseconds"];
            set => this["metricsStreamPollIntervalInMilliseconds"] = value;
        }

        [ConfigurationProperty("jsonConfigurationSourceOptions")]
        public HystrixJsonConfigurationSourceOptionsElement JsonConfigurationSourceOptions
        {
            get => this["jsonConfigurationSourceOptions"] as HystrixJsonConfigurationSourceOptionsElement;
            set => this["jsonConfigurationSourceOptions"] = value;
        }

        [ConfigurationProperty("localOptions")]
        public HystrixLocalOptionsElement LocalOptions
        {
            get => this["localOptions"] as HystrixLocalOptionsElement;
            set => this["localOptions"] = value;
        }
    }
}