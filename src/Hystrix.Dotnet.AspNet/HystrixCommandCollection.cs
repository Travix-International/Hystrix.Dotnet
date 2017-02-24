using System;
using System.Configuration;

namespace Hystrix.Dotnet.AspNet
{
    public class HystrixCommandCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new HystrixCommandElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            var command = element as HystrixCommandElement;
            if (command == null)
            {
                throw new InvalidCastException("element");
            }

            return command.Key;
        }
    }
}