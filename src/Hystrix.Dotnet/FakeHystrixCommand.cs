using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hystrix.Dotnet
{
    public class FakeHystrixCommand : IHystrixCommand
    {
        private readonly bool runFallbackOrThrowException;

        public HystrixCommandIdentifier CommandIdentifier { get; private set; }
        public IHystrixCircuitBreaker CircuitBreaker { get; private set; }
        public IHystrixCommandMetrics CommandMetrics { get; private set; }
        public IHystrixThreadPoolMetrics ThreadPoolMetrics { get; private set; }
        public IHystrixConfigurationService ConfigurationService { get; private set; }

        public FakeHystrixCommand(HystrixCommandIdentifier commandIdentifier, bool runFallbackOrThrowException)
        {
            this.runFallbackOrThrowException = runFallbackOrThrowException;
            CommandIdentifier = commandIdentifier;
        }

        public T Execute<T>(Func<T> primaryFunction, CancellationTokenSource cancellationTokenSource = null)
        {
            if (runFallbackOrThrowException)
            {
                throw new HystrixCommandException();
            }

            return primaryFunction.Invoke();
        }

        public T Execute<T>(Func<T> primaryFunction, Func<T> fallbackFunction, CancellationTokenSource cancellationTokenSource = null)
        {
            if (runFallbackOrThrowException)
            {
                return fallbackFunction.Invoke();
            }

            return primaryFunction.Invoke();
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> primaryFunction, CancellationTokenSource cancellationTokenSource = null)
        {
            if (runFallbackOrThrowException)
            {
                throw new HystrixCommandException();
            }

            return await primaryFunction.Invoke();
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> primaryFunction, Func<Task<T>> fallbackFunction, CancellationTokenSource cancellationTokenSource = null)
        {
            if (runFallbackOrThrowException)
            {
                return await fallbackFunction.Invoke();
            }

            return await primaryFunction.Invoke();
        }
    }
}