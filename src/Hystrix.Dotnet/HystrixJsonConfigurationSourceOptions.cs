namespace Hystrix.Dotnet
{
    public class HystrixJsonConfigurationSourceOptions
    {
        public int PollingIntervalInMilliseconds { get; set; }

        public string LocationPattern { get; set; }

        public string BaseLocation { get; set; }
    }
}