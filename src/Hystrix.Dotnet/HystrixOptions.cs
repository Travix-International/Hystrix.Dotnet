namespace Hystrix.Dotnet
{
    public class HystrixOptions
    {
        public static HystrixOptions CreateDefault()
        {
            return new HystrixOptions();
        }

        public string ConfigurationServiceImplementation { get; set; } = "HystrixLocalConfigurationService";

        public int MetricsStreamPollIntervalInMilliseconds { get; set; } = 500;

        public HystrixLocalOptions LocalOptions { get; set; } = HystrixLocalOptions.CreateDefault();

        public HystrixJsonConfigurationSourceOptions JsonConfigurationSourceOptions { get; set; }
    }
}