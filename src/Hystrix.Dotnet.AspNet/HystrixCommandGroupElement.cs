using System.Configuration;

namespace Hystrix.Dotnet.AspNet
{
    public class HystrixCommandGroupElement : ConfigurationElement
    {
        [ConfigurationProperty("key", IsRequired = true)]
        public string Key
        {
            get { return this["key"].ToString(); }
            set { this["key"] = value; }
        }

        [ConfigurationProperty("commands", IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(HystrixCommandElement), AddItemName = "add", ClearItemsName = "clear", RemoveItemName = "remove")]
        public HystrixCommandCollection Commands
        {
            get { return (HystrixCommandCollection)this["commands"]; }
            set { this["commands"] = value; }
        }
    }
}