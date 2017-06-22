using System;
using System.Threading;
using System.Threading.Tasks;
using Hystrix.Dotnet.Logging;

namespace Hystrix.Dotnet
{
    public class HystrixRetryWrapper : IHystrixRetryWrapper
    {
        private static readonly ILog log = LogProvider.GetLogger(typeof(HystrixTimeoutWrapper));

        private readonly HystrixCommandIdentifier commandIdentifier;
        private readonly IHystrixConfigurationService configurationService;

        public HystrixRetryWrapper(HystrixCommandIdentifier commandIdentifier, IHystrixConfigurationService configurationService)
        {
            this.commandIdentifier = commandIdentifier ?? throw new ArgumentNullException(nameof(commandIdentifier));
            this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        }

        public T Execute<T>(Func<T> primaryFunction, CancellationTokenSource cancellationTokenSource = null)
        {
            var retryCount = configurationService.GetCommandRetryCount();

            var errorCount = 0;

            while (true)
            {
                try
                {
                    return primaryFunction();
                }
                catch (Exception ex)
                {
                    errorCount++;

                    if (errorCount > retryCount)
                    {
                        throw;
                    }

                    log.InfoFormat(
                        "Retrying the command {0}-{1} (for the {2}. out of {3} times) due to the following error: {4}.",
                        commandIdentifier.GroupKey,
                        commandIdentifier.CommandKey,
                        errorCount,
                        retryCount,
                        ex);
                }
            }
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> primaryFunction, CancellationTokenSource cancellationTokenSource = null)
        {
            var retryCount = configurationService.GetCommandRetryCount();

            var errorCount = 0;

            while (true)
            {
                try
                {
                    return await primaryFunction();
                }
                catch (Exception ex)
                {
                    errorCount++;

                    if (errorCount > retryCount)
                    {
                        throw;
                    }

                    log.InfoFormat(
                        "Retrying the command {0}-{1} (for the {2}. out of {3} times) due to the following error: {4}.",
                        commandIdentifier.GroupKey,
                        commandIdentifier.CommandKey,
                        errorCount,
                        retryCount,
                        ex);
                }
            }
        }
    }
}