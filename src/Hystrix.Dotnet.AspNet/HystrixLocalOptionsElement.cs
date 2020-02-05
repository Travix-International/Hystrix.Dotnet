using System.Configuration;

namespace Hystrix.Dotnet.AspNet
{
    public class HystrixLocalOptionsElement : ConfigurationElement
    {
        [ConfigurationProperty("commandGroups", IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(HystrixCommandGroupElement), AddItemName = "add", ClearItemsName = "clear", RemoveItemName = "remove")]
        public HystrixCommandGroupCollection CommandGroups
        {
            get => (HystrixCommandGroupCollection)this["commandGroups"];
            set => this["commandGroups"] = value;
        }

        [ConfigurationProperty("defaultConfiguration", DefaultValue = null)]
        public HystrixDefaultConfigurationElement DefaultConfiguration
        {
            get => (HystrixDefaultConfigurationElement)this["defaultConfiguration"];
            set => this["defaultConfiguration"] = value;
        }
    }
}