namespace Hystrix.Dotnet
{
    public class HystrixOptions
    {
        public HystrixOptions()
        {
            ConfigurationServiceImplementation = "HystrixLocalConfigurationService";
            MetricsStreamPollIntervalInMilliseconds = 500;
            LocalOptions = HystrixLocalOptions.CreateDefault();
        }

        public static HystrixOptions CreateDefault()
        {
            return new HystrixOptions();
        }

        public string ConfigurationServiceImplementation { get; set; }

        public int MetricsStreamPollIntervalInMilliseconds { get; set; }

        public HystrixJsonConfigurationSourceOptions  JsonConfigurationSourceOptions { get; set; }

        public HystrixLocalOptions LocalOptions { get; set; }
    }
}