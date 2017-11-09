using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hystrix.Dotnet
{
    public interface IHystrixTimeoutWrapper
    {
        /// <summary>
        /// Executes synchronous <paramref name="primaryFunction"/> against a timeout value fetched internally; a failure to execute within that time results in a <see cref="HystrixCommandException"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="primaryFunction"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        T Execute<T>(Func<T> primaryFunction, CancellationTokenSource cancellationTokenSource = null);

        /// <summary>
        /// Executes asynchronous <paramref name="primaryTask"/> against a timeout value fetched internally; a failure to execute within that time results in a <see cref="HystrixCommandException"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="primaryTask"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        Task<T> ExecuteAsync<T>(Func<Task<T>> primaryTask, CancellationTokenSource cancellationTokenSource = null);
    }
}