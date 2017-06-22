﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Hystrix.Dotnet.Logging;

namespace Hystrix.Dotnet
{
    public class HystrixTimeoutWrapper : IHystrixTimeoutWrapper
    {
        private static readonly ILog log = LogProvider.GetLogger(typeof(HystrixTimeoutWrapper));

        private readonly HystrixCommandIdentifier commandIdentifier;
        private readonly IHystrixConfigurationService configurationService;

        public HystrixTimeoutWrapper(HystrixCommandIdentifier commandIdentifier, IHystrixConfigurationService configurationService)
        {
            this.commandIdentifier = commandIdentifier ?? throw new ArgumentNullException(nameof(commandIdentifier));
            this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        }

        /// <inheritdoc/>
        public T Execute<T>(Func<T> primaryFunction, CancellationTokenSource cancellationTokenSource = null)
        {
            var timeout = configurationService.GetCommandTimeoutInMilliseconds();

            var outerTask = Task.Run(() => primaryFunction.Invoke());

            try
            {
                if (outerTask.Wait(timeout))
                {
                    // task completed within timeout; use .GetAwaiter().GetResult() instead of .Result to avoid exceptions being wrapped in an AggregateException
                    return outerTask.Result;
                }
            }
            catch (AggregateException ae)
            {                
                // unwrap exception, the Wait or Result wraps it in an AggregateException
                throw ae.InnerException;
            }

            // primaryFunction continues to occupy a thread until it finishes, although nothing will be done with the result; unless we have a cancellationToken and can cancel the task
            if (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
            }

            // timeout, no fallback
            log.WarnFormat("Executing sync function has timed out for group {0} and key {1}.", commandIdentifier.GroupKey, commandIdentifier.CommandKey);

            throw new HystrixTimeoutException();
        }

        /// <inheritdoc/>
        public async Task<T> ExecuteAsync<T>(Func<Task<T>> primaryTask, CancellationTokenSource cancellationTokenSource = null)
        {
            var timeout = configurationService.GetCommandTimeoutInMilliseconds();

            var timeoutCancellationTokenSource = new CancellationTokenSource();

            // wrap in a task so it doesn't wait for any non-awaitable parts of the primaryTask
            // ReSharper disable once MethodSupportsCancellation
            var outerTask = Task.Run(primaryTask.Invoke);

            if (await Task.WhenAny(outerTask, Task.Delay(timeout, timeoutCancellationTokenSource.Token)).ConfigureAwait(false) == outerTask)
            {
                // make sure Task.Delay stops
                timeoutCancellationTokenSource.Cancel();

                // task completed within timeout
                return await outerTask.ConfigureAwait(false);
            }

            // primaryTask continues to run until it finishes, although nothing will be done with the result; it won't block any threads as it's async, but still consumes resources; unless we have a cancellationToken
            if (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
            }

            // timeout, no fallback
            log.WarnFormat("Executing async task has timed out for group {0} and key {1}.", commandIdentifier.GroupKey, commandIdentifier.CommandKey);

            throw new HystrixTimeoutException();
        }
    }
}
