using System;
using System.Configuration;

namespace Hystrix.Dotnet.AspNet
{
    public class HystrixCommandGroupCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new HystrixCommandGroupElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            var commandGroup = element as HystrixCommandGroupElement;
            if (commandGroup == null)
            {
                throw new InvalidCastException("element");
            }

            return commandGroup.Key;
        }
    }
}