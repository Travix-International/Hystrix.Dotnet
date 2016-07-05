using System.Collections.Generic;

namespace Hystrix.Dotnet
{
    public interface IHystrixCommandFactory
    {
        IHystrixCommand GetHystrixCommand(HystrixCommandIdentifier commandIdentifier);
        IHystrixCommand GetHystrixCommand(string groupKey, string commandKey);
        ICollection<IHystrixCommand> GetAllHystrixCommands();
    }
}