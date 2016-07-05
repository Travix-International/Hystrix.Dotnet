using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Hystrix.Dotnet
{
    public class FakeHystrixCommandFactory: IHystrixCommandFactory
    {
        private readonly bool runFallbackOrThrowException;

        public FakeHystrixCommandFactory(bool runFallbackOrThrowException = false)
        {
            this.runFallbackOrThrowException = runFallbackOrThrowException;
        }

        public IHystrixCommand GetHystrixCommand(HystrixCommandIdentifier commandIdentifier)
        {
            return new FakeHystrixCommand(commandIdentifier, runFallbackOrThrowException);
        }

        public IHystrixCommand GetHystrixCommand(string groupKey, string commandKey)
        {
            return GetHystrixCommand(new HystrixCommandIdentifier(groupKey, commandKey));
        }

        public ICollection<IHystrixCommand> GetAllHystrixCommands()
        {
            return new Collection<IHystrixCommand>();
        }

        public IHystrixThreadPoolMetrics GetThreadPoolMetrics()
        {
            throw new System.NotImplementedException();
        }
    }
}
