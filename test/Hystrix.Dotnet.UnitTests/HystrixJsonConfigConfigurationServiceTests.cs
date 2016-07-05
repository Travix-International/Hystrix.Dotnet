using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Hystrix.Dotnet.UnitTests
{
    public class HystrixJsonConfigConfigurationServiceTests
    {
        public class Constructor
        {
            [Fact]
            public void Throws_ArgumentNullException_When_CommandIdentifier_Is_Null()
            {
                // act
                Assert.Throws<ArgumentNullException>(() => new HystrixJsonConfigConfigurationService(null));
            }
        }
    }

    public class GetCommandTimeoutInMilliseconds
    {
        [Fact]
        public void Returns_Remote_Config_Value_From_File_Scheme()
        {
            SetLocalFileBaseLocation();
            ConfigurationManager.AppSettings["HystrixJsonConfigConfigurationService-LocationPattern"] = "{0}-{1}.json";

            var commandIdentifier = new HystrixCommandIdentifier("Group", "Command");
            var configurationService = new HystrixJsonConfigConfigurationService(commandIdentifier);

            // act
            var value = configurationService.GetCommandTimeoutInMilliseconds();

            Assert.Equal(25000, value);
        }

        [Fact(Skip = "Is an integration, not unit test")]
        public void Returns_Remote_Config_Value_From_Http_Scheme_Json_File()
        {
            ConfigurationManager.AppSettings["HystrixJsonConfigConfigurationService-BaseLocation"] = "http://hystrix-configuration.staging.travix.com/";
            ConfigurationManager.AppSettings["HystrixJsonConfigConfigurationService-LocationPattern"] = "{0}-{1}.json";

            var commandIdentifier = new HystrixCommandIdentifier("Group", "Command");
            var configurationService = new HystrixJsonConfigConfigurationService(commandIdentifier);

            // act
            var value = configurationService.GetCommandTimeoutInMilliseconds();

            Assert.Equal(25000, value);
        }

        [Fact(Skip = "Is an integration, not unit test")]
        public void Returns_Values_From_Default_Json_If_Specific_Json_Is_Not_Present_Or_Invalid()
        {
            ConfigurationManager.AppSettings["HystrixJsonConfigConfigurationService-BaseLocation"] = "http://hystrix-configuration.staging.travix.com/";
            ConfigurationManager.AppSettings["HystrixJsonConfigConfigurationService-LocationPattern"] = "{0}-{1}.json";

            var commandIdentifier = new HystrixCommandIdentifier("NoExisting", "Command");
            var configurationService = new HystrixJsonConfigConfigurationService(commandIdentifier);

            // act
            var value = configurationService.GetCommandTimeoutInMilliseconds();

            Assert.Equal(60000, value);
        }
        
        private static void SetLocalFileBaseLocation()
        {
            ConfigurationManager.AppSettings["HystrixJsonConfigConfigurationService-BaseLocation"] = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + System.IO.Path.DirectorySeparatorChar).AbsoluteUri;
        }
    }

    public class Threading
    {
        [Fact(Skip = "Does not work properly when tests are running concurrently")]
        public async Task Costs_Up_To_2_ThreadPool_Threads_During_Background_Task()
        {
            var cancellationTokenSource = new CancellationTokenSource();

            int availableThreadsBefore = GetAvailableThreads();

            // act
            var task = Task.Run(async () =>
            {
                while (true)
                {
                    if (cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        break;
                    }

                    // wait for an interval with jitter
                    await Task.Delay(1000, cancellationTokenSource.Token).ConfigureAwait(false);
                }
            }, cancellationTokenSource.Token);

            await Task.Delay(100);

            int availableThreadsDuring = GetAvailableThreads();

            Assert.InRange(availableThreadsBefore - availableThreadsDuring, 0, 2);

            cancellationTokenSource.Cancel();

            //await task;

            int availableThreadsAfter = GetAvailableThreads();

            Assert.Equal(0, availableThreadsBefore - availableThreadsAfter);
        }

        [Fact(Skip = "Does not work properly when tests are running concurrently")]
        public async Task Costs_0_ThreadPool_Threads_During_Background_Task_With_LongRunning()
        {
            var cancellationTokenSource = new CancellationTokenSource();

            int availableThreadsBefore = GetAvailableThreads();

            // act
            var task = Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    if (cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        break;
                    }

                    // wait for an interval with jitter
                    await Task.Delay(1000, cancellationTokenSource.Token).ConfigureAwait(false);
                }
            },
                cancellationTokenSource.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);

            int availableThreadsDuring = GetAvailableThreads();

            Assert.Equal(0, availableThreadsBefore - availableThreadsDuring);

            cancellationTokenSource.Cancel();

            await task;

            int availableThreadsAfter = GetAvailableThreads();

            Assert.Equal(0, availableThreadsBefore - availableThreadsAfter);
        }

        private int GetAvailableThreads()
        {
            int availableWorkerThreads;
            int availableCompletionPortThreads;

            ThreadPool.GetAvailableThreads(out availableWorkerThreads, out availableCompletionPortThreads);

            return availableWorkerThreads + availableCompletionPortThreads;
        }
    }
}
