using System.Configuration;

namespace Hystrix.Dotnet.AspNet
{
    public class HystrixLocalOptionsElement : ConfigurationElement
    {
        [ConfigurationProperty("commandGroups", IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(HystrixCommandGroupElement), AddItemName = "add", ClearItemsName = "clear", RemoveItemName = "remove")]
        public HystrixCommandGroupCollection CommandGroups
        {
            get { return (HystrixCommandGroupCollection)this["commandGroups"]; }
            set { this["commandGroups"] = value; }
        }
    }
}