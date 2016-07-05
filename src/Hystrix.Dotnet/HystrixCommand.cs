using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Hystrix.Dotnet
{
    public class HystrixCommand : IHystrixCommand
    {
        private readonly HystrixCommandIdentifier commandIdentifier;
        private readonly IHystrixTimeoutWrapper timeoutWrapper;
        private readonly IHystrixCircuitBreaker circuitBreaker;
        private readonly IHystrixCommandMetrics commandMetrics;
        private readonly IHystrixThreadPoolMetrics threadPoolMetrics;
        private readonly IHystrixConfigurationService configurationService;

        public HystrixCommandIdentifier CommandIdentifier { get { return commandIdentifier; } }

        public IHystrixCircuitBreaker CircuitBreaker { get { return circuitBreaker; } }

        public IHystrixCommandMetrics CommandMetrics { get { return commandMetrics; } }

        public IHystrixThreadPoolMetrics ThreadPoolMetrics { get { return threadPoolMetrics; } }

        public IHystrixConfigurationService ConfigurationService { get { return configurationService; } }

        public HystrixCommand(HystrixCommandIdentifier commandIdentifier, IHystrixTimeoutWrapper timeoutWrapper, IHystrixCircuitBreaker circuitBreaker, IHystrixCommandMetrics commandMetrics, IHystrixThreadPoolMetrics threadPoolMetrics, IHystrixConfigurationService configurationService)
        {
            if (commandIdentifier == null)
            {
                throw new ArgumentNullException("commandIdentifier");
            }
            if (timeoutWrapper == null)
            {
                throw new ArgumentNullException("timeoutWrapper");
            }
            if (circuitBreaker == null)
            {
                throw new ArgumentNullException("circuitBreaker");
            }
            if (commandMetrics == null)
            {
                throw new ArgumentNullException("commandMetrics");
            }
            if (threadPoolMetrics == null)
            {
                throw new ArgumentNullException("threadPoolMetrics");
            }
            if (configurationService == null)
            {
                throw new ArgumentNullException("configurationService");
            }

            this.commandIdentifier = commandIdentifier;
            this.timeoutWrapper = timeoutWrapper;
            this.circuitBreaker = circuitBreaker;
            this.commandMetrics = commandMetrics;
            this.threadPoolMetrics = threadPoolMetrics;
            this.configurationService = configurationService;
        }

        /// <inheritdoc/>
        public T Execute<T>(Func<T> primaryFunction, CancellationTokenSource cancellationTokenSource = null)
        {
            return InnerExecute(
                primaryFunction,
                innerExceptions => { throw new HystrixCommandException(innerExceptions); },
                cancellationTokenSource);
        }

        /// <inheritdoc/>
        public T Execute<T>(Func<T> primaryFunction, Func<T> fallbackFunction, CancellationTokenSource cancellationTokenSource = null)
        {
            return InnerExecute(
                primaryFunction,
                innerExceptions => fallbackFunction.Invoke(),
                cancellationTokenSource);
        }

        private T InnerExecute<T>(Func<T> primaryFunction, Func<IEnumerable<Exception>, T> fallbackFunction, CancellationTokenSource cancellationTokenSource)
        {
            if (!configurationService.GetHystrixCommandEnabled())
            {
                return primaryFunction.Invoke();
            }

            var innerExceptions = new List<Exception>();

            Stopwatch userThreadStopWatch = Stopwatch.StartNew();

            if (circuitBreaker.AllowRequest())
            {
                commandMetrics.IncrementConcurrentExecutionCount();
                threadPoolMetrics.MarkThreadExecution();

                Stopwatch commandStopWatch = Stopwatch.StartNew();

                try
                {
                    var result = timeoutWrapper.Execute(primaryFunction, cancellationTokenSource);
                    
                    commandStopWatch.Stop();

                    circuitBreaker.CloseCircuit();

                    commandMetrics.MarkSuccess();

                    return result;
                }
                catch (HystrixTimeoutException hte)
                {
                    commandStopWatch.Stop();
                    commandMetrics.MarkTimeout();
                    innerExceptions.Add(hte);
                }
                catch (Exception ex)
                {
                    commandStopWatch.Stop();
                    commandMetrics.MarkFailure();
                    commandMetrics.MarkExceptionThrown();
                    innerExceptions.Add(ex);
                }
                finally
                {
                    // track command execution time
                    commandStopWatch.Stop();
                    commandMetrics.AddCommandExecutionTime(commandStopWatch.Elapsed.TotalMilliseconds);

                    commandMetrics.DecrementConcurrentExecutionCount();
                    threadPoolMetrics.MarkThreadCompletion();

                    // track execution time including threading overhead
                    userThreadStopWatch.Stop();
                    commandMetrics.AddUserThreadExecutionTime(userThreadStopWatch.Elapsed.TotalMilliseconds);
                }
            }
            else
            {
                commandMetrics.MarkShortCircuited();

                // track execution time including threading overhead
                userThreadStopWatch.Stop();
                commandMetrics.AddUserThreadExecutionTime(userThreadStopWatch.Elapsed.TotalMilliseconds);
            }

            T fallbackResult = fallbackFunction.Invoke(innerExceptions);
            commandMetrics.MarkFallbackSuccess();

            return fallbackResult;
        }

        /// <inheritdoc/>
        public async Task<T> ExecuteAsync<T>(Func<Task<T>> primaryFunction, CancellationTokenSource cancellationTokenSource = null)
        {
            return await InnerExecuteAsync(
                primaryFunction,
                innerExceptions => { throw new HystrixCommandException(innerExceptions); },
                cancellationTokenSource).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<T> ExecuteAsync<T>(Func<Task<T>> primaryFunction, Func<Task<T>> fallbackFunction, CancellationTokenSource cancellationTokenSource = null)
        {
            return await InnerExecuteAsync(
                primaryFunction,
                innerExceptions => fallbackFunction,
                cancellationTokenSource).ConfigureAwait(false);
        }

        private async Task<T> InnerExecuteAsync<T>(Func<Task<T>> primaryFunction, Func<IEnumerable<Exception>, Func<Task<T>>> fallbackFunction, CancellationTokenSource cancellationTokenSource)
        {
            if (!configurationService.GetHystrixCommandEnabled())
            {
                return await primaryFunction.Invoke().ConfigureAwait(false);
            }

            var innerExceptions = new List<Exception>();

            Stopwatch userThreadStopWatch = Stopwatch.StartNew();

            if (circuitBreaker.AllowRequest())
            {
                commandMetrics.IncrementConcurrentExecutionCount();
                threadPoolMetrics.MarkThreadExecution();

                Stopwatch commandStopWatch = Stopwatch.StartNew();

                try
                {
                    var result = await timeoutWrapper.ExecuteAsync(primaryFunction, cancellationTokenSource).ConfigureAwait(false);
                    
                    commandStopWatch.Stop();

                    circuitBreaker.CloseCircuit();

                    commandMetrics.MarkSuccess();

                    return result;
                }
                catch (HystrixTimeoutException hte)
                {
                    commandStopWatch.Stop();
                    commandMetrics.MarkTimeout();
                    innerExceptions.Add(hte);
                }
                catch (Exception ex)
                {
                    commandStopWatch.Stop();
                    commandMetrics.MarkFailure();
                    commandMetrics.MarkExceptionThrown();
                    innerExceptions.Add(ex);
                }
                finally
                {
                    // track command execution time
                    commandStopWatch.Stop();
                    commandMetrics.AddCommandExecutionTime(commandStopWatch.Elapsed.TotalMilliseconds);

                    commandMetrics.DecrementConcurrentExecutionCount();
                    threadPoolMetrics.MarkThreadCompletion();

                    // track execution time including threading overhead
                    userThreadStopWatch.Stop();
                    commandMetrics.AddUserThreadExecutionTime(userThreadStopWatch.Elapsed.TotalMilliseconds);
                }
            }
            else
            {
                commandMetrics.MarkShortCircuited();

                // track execution time including threading overhead
                userThreadStopWatch.Stop();
                commandMetrics.AddUserThreadExecutionTime(userThreadStopWatch.Elapsed.TotalMilliseconds);
            }

            T fallbackResult = await fallbackFunction.Invoke(innerExceptions).Invoke().ConfigureAwait(false);
            commandMetrics.MarkFallbackSuccess();

            return fallbackResult;
        }
    }
}