using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hystrix.Dotnet
{
    public interface IHystrixRetryWrapper
    {
        T Execute<T>(Func<T> primaryFunction, CancellationTokenSource cancellationTokenSource = null);

        Task<T> ExecuteAsync<T>(Func<Task<T>> primaryFunction, CancellationTokenSource cancellationTokenSource = null);
    }
}