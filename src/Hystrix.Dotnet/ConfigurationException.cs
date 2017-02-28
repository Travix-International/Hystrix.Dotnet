using System;

namespace Hystrix.Dotnet
{
    public class ConfigurationException : Exception
    {
        public ConfigurationException(string msg) : base(msg)
        {
        }
    }
}