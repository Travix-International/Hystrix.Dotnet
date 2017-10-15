using System.Configuration;

namespace Hystrix.Dotnet.WebConfiguration
{
    public class HystrixCommandGroupCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new HystrixCommandGroupElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((HystrixCommandGroupElement) element).Key;
        }
    }
}