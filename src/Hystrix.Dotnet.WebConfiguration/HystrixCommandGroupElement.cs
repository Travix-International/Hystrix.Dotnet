using System.Configuration;

namespace Hystrix.Dotnet.WebConfiguration
{
    public class HystrixCommandGroupElement : ConfigurationElement
    {
        [ConfigurationProperty("key", IsRequired = true)]
        public string Key
        {
            get => this["key"].ToString();
            set => this["key"] = value;
        }

        [ConfigurationProperty("commands", IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(HystrixCommandElement), AddItemName = "add", ClearItemsName = "clear", RemoveItemName = "remove")]
        public HystrixCommandCollection Commands
        {
            get => (HystrixCommandCollection)this["commands"];
            set => this["commands"] = value;
        }
    }
}