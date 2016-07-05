using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hystrix.Dotnet
{
    internal interface IInternalHystrixCommand
    {
        /// <summary>
        /// Returns the identifier
        /// </summary>
        HystrixCommandIdentifier CommandIdentifier { get; }

        /// <summary>
        /// Returns the internal circuit breaker
        /// </summary>
        IHystrixCircuitBreaker CircuitBreaker { get; }

        /// <summary>
        /// Exposes the metrics
        /// </summary>
        IHystrixCommandMetrics CommandMetrics { get; }

        /// <summary>
        /// Runs the synchronous <see cref="primaryFunction"/> wrapped by circuit breaker and timeout pattern, without a fallback; an error, timeout or open circuit breaker will result in a <see cref="HystrixCommandException"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="primaryFunction"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        T Execute<T>(Func<T> primaryFunction, CancellationTokenSource cancellationTokenSource = null);

        /// <summary>
        /// Runs the synchronous <see cref="primaryFunction"/> wrapped by circuit breaker and timeout pattern, with a fallback function; an error, timeout or open circuit breaker will execute the <see cref="fallbackFunction"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="primaryFunction"></param>
        /// <param name="fallbackFunction"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        T Execute<T>(Func<T> primaryFunction, Func<T> fallbackFunction, CancellationTokenSource cancellationTokenSource = null);

        /// <summary>
        /// Runs the asynchronous <see cref="primaryFunction"/> wrapped by circuit breaker and timeout pattern, without a fallback; an error, timeout or open circuit breaker will result in a <see cref="HystrixCommandException"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="primaryFunction"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        Task<T> ExecuteAsync<T>(Func<Task<T>> primaryFunction, CancellationTokenSource cancellationTokenSource = null);

        /// <summary>
        /// Runs the asynchronous <see cref="primaryFunction"/> wrapped by circuit breaker and timeout pattern, with a fallback function; an error, timeout or open circuit breaker will execute the <see cref="fallbackFunction"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="primaryFunction"></param>
        /// <param name="fallbackFunction"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        Task<T> ExecuteAsync<T>(Func<Task<T>> primaryFunction, Func<Task<T>> fallbackFunction, CancellationTokenSource cancellationTokenSource = null);
    }
}