using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Hystrix.Dotnet
{
    public class HystrixCommand : IHystrixCommand
    {
        private readonly IHystrixTimeoutWrapper timeoutWrapper;

        public HystrixCommandIdentifier CommandIdentifier { get; }

        public IHystrixCircuitBreaker CircuitBreaker { get; }

        public IHystrixCommandMetrics CommandMetrics { get; }

        public IHystrixThreadPoolMetrics ThreadPoolMetrics { get; }

        public IHystrixConfigurationService ConfigurationService { get; }

        public HystrixCommand(HystrixCommandIdentifier commandIdentifier, IHystrixTimeoutWrapper timeoutWrapper, IHystrixCircuitBreaker circuitBreaker, IHystrixCommandMetrics commandMetrics, IHystrixThreadPoolMetrics threadPoolMetrics, IHystrixConfigurationService configurationService)
        {
            if (commandIdentifier == null)
            {
                throw new ArgumentNullException(nameof(commandIdentifier));
            }

            if (timeoutWrapper == null)
            {
                throw new ArgumentNullException(nameof(timeoutWrapper));
            }

            if (circuitBreaker == null)
            {
                throw new ArgumentNullException(nameof(circuitBreaker));
            }

            if (commandMetrics == null)
            {
                throw new ArgumentNullException(nameof(commandMetrics));
            }

            if (threadPoolMetrics == null)
            {
                throw new ArgumentNullException(nameof(threadPoolMetrics));
            }

            if (configurationService == null)
            {
                throw new ArgumentNullException(nameof(configurationService));
            }

            CommandIdentifier = commandIdentifier;
            this.timeoutWrapper = timeoutWrapper;
            CircuitBreaker = circuitBreaker;
            CommandMetrics = commandMetrics;
            ThreadPoolMetrics = threadPoolMetrics;
            ConfigurationService = configurationService;
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
            if (!ConfigurationService.GetHystrixCommandEnabled())
            {
                return primaryFunction.Invoke();
            }

            var innerExceptions = new List<Exception>();

            Stopwatch userThreadStopWatch = Stopwatch.StartNew();

            if (CircuitBreaker.AllowRequest())
            {
                CommandMetrics.IncrementConcurrentExecutionCount();
                ThreadPoolMetrics.MarkThreadExecution();

                Stopwatch commandStopWatch = Stopwatch.StartNew();

                try
                {
                    var result = timeoutWrapper.Execute(primaryFunction, cancellationTokenSource);
                    
                    commandStopWatch.Stop();

                    CircuitBreaker.CloseCircuit();

                    CommandMetrics.MarkSuccess();

                    return result;
                }
                catch (HystrixTimeoutException hte)
                {
                    commandStopWatch.Stop();
                    CommandMetrics.MarkTimeout();
                    innerExceptions.Add(hte);
                }
                catch (Exception ex)
                {
                    commandStopWatch.Stop();
                    CommandMetrics.MarkFailure();
                    CommandMetrics.MarkExceptionThrown();
                    innerExceptions.Add(ex);
                }
                finally
                {
                    // track command execution time
                    commandStopWatch.Stop();
                    CommandMetrics.AddCommandExecutionTime(commandStopWatch.Elapsed.TotalMilliseconds);

                    CommandMetrics.DecrementConcurrentExecutionCount();
                    ThreadPoolMetrics.MarkThreadCompletion();

                    // track execution time including threading overhead
                    userThreadStopWatch.Stop();
                    CommandMetrics.AddUserThreadExecutionTime(userThreadStopWatch.Elapsed.TotalMilliseconds);
                }
            }
            else
            {
                CommandMetrics.MarkShortCircuited();

                // track execution time including threading overhead
                userThreadStopWatch.Stop();
                CommandMetrics.AddUserThreadExecutionTime(userThreadStopWatch.Elapsed.TotalMilliseconds);
            }

            T fallbackResult = fallbackFunction.Invoke(innerExceptions);
            CommandMetrics.MarkFallbackSuccess();

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
            if (!ConfigurationService.GetHystrixCommandEnabled())
            {
                return await primaryFunction.Invoke().ConfigureAwait(false);
            }

            var innerExceptions = new List<Exception>();

            Stopwatch userThreadStopWatch = Stopwatch.StartNew();

            if (CircuitBreaker.AllowRequest())
            {
                CommandMetrics.IncrementConcurrentExecutionCount();
                ThreadPoolMetrics.MarkThreadExecution();

                Stopwatch commandStopWatch = Stopwatch.StartNew();

                try
                {
                    var result = await timeoutWrapper.ExecuteAsync(primaryFunction, cancellationTokenSource).ConfigureAwait(false);
                    
                    commandStopWatch.Stop();

                    CircuitBreaker.CloseCircuit();

                    CommandMetrics.MarkSuccess();

                    return result;
                }
                catch (HystrixTimeoutException hte)
                {
                    commandStopWatch.Stop();
                    CommandMetrics.MarkTimeout();
                    innerExceptions.Add(hte);
                }
                catch (Exception ex)
                {
                    commandStopWatch.Stop();
                    CommandMetrics.MarkFailure();
                    CommandMetrics.MarkExceptionThrown();
                    innerExceptions.Add(ex);
                }
                finally
                {
                    // track command execution time
                    commandStopWatch.Stop();
                    CommandMetrics.AddCommandExecutionTime(commandStopWatch.Elapsed.TotalMilliseconds);

                    CommandMetrics.DecrementConcurrentExecutionCount();
                    ThreadPoolMetrics.MarkThreadCompletion();

                    // track execution time including threading overhead
                    userThreadStopWatch.Stop();
                    CommandMetrics.AddUserThreadExecutionTime(userThreadStopWatch.Elapsed.TotalMilliseconds);
                }
            }
            else
            {
                CommandMetrics.MarkShortCircuited();

                // track execution time including threading overhead
                userThreadStopWatch.Stop();
                CommandMetrics.AddUserThreadExecutionTime(userThreadStopWatch.Elapsed.TotalMilliseconds);
            }

            T fallbackResult = await fallbackFunction.Invoke(innerExceptions).Invoke().ConfigureAwait(false);
            CommandMetrics.MarkFallbackSuccess();

            return fallbackResult;
        }
    }
}