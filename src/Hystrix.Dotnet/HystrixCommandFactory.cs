using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Hystrix.Dotnet
{
    public class HystrixCommandFactory : IHystrixCommandFactory
    {
        //private static readonly ILog Log = LogManager.GetLogger(typeof(HystrixCommandFactory));
        private static readonly ConcurrentDictionary<HystrixCommandIdentifier, IHystrixCommand> CommandsDictionary = new ConcurrentDictionary<HystrixCommandIdentifier, IHystrixCommand>();

        private const string ConfigurationserviceimplementationAppsettingName = "HystrixCommandFactory-ConfigurationServiceImplementation";

        public IHystrixCommand GetHystrixCommand(HystrixCommandIdentifier commandIdentifier)
        {
            IHystrixCommand hystrixCommand;
            if (CommandsDictionary.TryGetValue(commandIdentifier, out hystrixCommand))
            {
                return hystrixCommand;
            }

            // add value in a thread-safe way
            hystrixCommand = CommandsDictionary.AddOrUpdate(
                commandIdentifier,
                ci =>
                {
                    //Log.DebugFormat("Added a new command with group {0} and key {1}.", ci.GroupKey, ci.CommandKey);
                    return CreateHystrixCommand(ci);
                },
                (ci, command) =>
                {
                    //Log.DebugFormat("Command with group {0} and key {1} already exists, not creating it again.", ci.GroupKey, ci.CommandKey);
                    return command;
                });

            return hystrixCommand;
        }

        public IHystrixCommand GetHystrixCommand(string groupKey, string commandKey)
        {
            return GetHystrixCommand(new HystrixCommandIdentifier(groupKey, commandKey));
        }

        private static IHystrixCommand CreateHystrixCommand(HystrixCommandIdentifier commandIdentifier)
        {
            var configurationServiceImplementation = ConfigurationManager.AppSettings[ConfigurationserviceimplementationAppsettingName];

            var configurationService = configurationServiceImplementation != null && configurationServiceImplementation.Equals("HystrixJsonConfigConfigurationService", StringComparison.OrdinalIgnoreCase)
                ? (IHystrixConfigurationService)new HystrixJsonConfigConfigurationService(commandIdentifier) : (IHystrixConfigurationService)new HystrixWebConfigConfigurationService(commandIdentifier);

            var commandMetrics = new HystrixCommandMetrics(commandIdentifier, configurationService);
            var timeoutWrapper = new HystrixTimeoutWrapper(commandIdentifier, configurationService);
            var circuitBreaker = new HystrixCircuitBreaker(commandIdentifier, configurationService, commandMetrics);
            var threadPoolMetrics = new HystrixThreadPoolMetrics(commandIdentifier, configurationService);

            return new HystrixCommand(commandIdentifier, timeoutWrapper, circuitBreaker, commandMetrics, threadPoolMetrics, configurationService);
        }
   
        public ICollection<IHystrixCommand> GetAllHystrixCommands()
        {
            return CommandsDictionary.Values.Where(x => x.ConfigurationService.GetHystrixCommandEnabled()).ToList();
        }

        /// <summary>
        /// Empty the dictionary, for testing purposes; this method is not exposed in the interface
        /// </summary>
        public static void Clear()
        {
            CommandsDictionary.Clear();
        }
    }
}
