using System.Configuration;

namespace Hystrix.Dotnet.AspNet
{
    public class HystrixJsonConfigurationSourceOptionsElement : ConfigurationElement
    {
        [ConfigurationProperty("pollingIntervalInMilliseconds", IsRequired = false, DefaultValue = 60000)]
        public int PollingIntervalInMilliseconds 
        {
            get { return (int)this["pollingIntervalInMilliseconds"]; }
            set { this["pollingIntervalInMilliseconds"] = value; }
        }

        [ConfigurationProperty("locationPattern", IsRequired = true)]
        public string LocationPattern
        {
            get { return this["locationPattern"].ToString(); }
            set { this["locationPattern"] = value; }
        }

        [ConfigurationProperty("baseLocation", IsRequired = true)]
        public string BaseLocation
        {
            get { return this["baseLocation"].ToString(); }
            set { this["baseLocation"] = value; }
        }
    }
}