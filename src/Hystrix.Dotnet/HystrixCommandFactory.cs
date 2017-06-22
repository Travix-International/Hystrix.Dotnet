﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Hystrix.Dotnet.Logging;

namespace Hystrix.Dotnet
{
    public class HystrixCommandFactory : IHystrixCommandFactory
    {
        private static readonly ILog log = LogProvider.GetLogger(typeof(HystrixCommandFactory));
        private static readonly ConcurrentDictionary<HystrixCommandIdentifier, IHystrixCommand> commandsDictionary = new ConcurrentDictionary<HystrixCommandIdentifier, IHystrixCommand>();

        private readonly HystrixOptions options;

        public HystrixCommandFactory(HystrixOptions options)
        {
            if (options == null)
            {
                throw new ArgumentException("The HystrixOptions must be specified in order to use Hystrix.Dotnet", nameof(options));
            }

            this.options = options;
        }

        public IHystrixCommand GetHystrixCommand(HystrixCommandIdentifier commandIdentifier)
        {
            IHystrixCommand hystrixCommand;
            if (commandsDictionary.TryGetValue(commandIdentifier, out hystrixCommand))
            {
                return hystrixCommand;
            }

            // add value in a thread-safe way
            hystrixCommand = commandsDictionary.AddOrUpdate(
                commandIdentifier,
                ci =>
                {
                    log.DebugFormat("Added a new command with group {0} and key {1}.", ci.GroupKey, ci.CommandKey);
                    return CreateHystrixCommand(ci, options);
                },
                (ci, command) =>
                {
                    log.DebugFormat("Command with group {0} and key {1} already exists, not creating it again.", ci.GroupKey, ci.CommandKey);
                    return command;
                });

            return hystrixCommand;
        }

        public IHystrixCommand GetHystrixCommand(string groupKey, string commandKey)
        {
            return GetHystrixCommand(new HystrixCommandIdentifier(groupKey, commandKey));
        }

        private static IHystrixCommand CreateHystrixCommand(HystrixCommandIdentifier commandIdentifier, HystrixOptions options)
        {
            var configurationServiceImplementation = options.ConfigurationServiceImplementation;

            var configurationService =
                configurationServiceImplementation != null && configurationServiceImplementation.Equals("HystrixJsonConfigConfigurationService", StringComparison.OrdinalIgnoreCase)
                ? (IHystrixConfigurationService)new HystrixJsonConfigConfigurationService(commandIdentifier, options.JsonConfigurationSourceOptions)
                : new HystrixLocalConfigurationService(commandIdentifier, options.LocalOptions);

            var commandMetrics = new HystrixCommandMetrics(commandIdentifier, configurationService);
            var timeoutWrapper = new HystrixTimeoutWrapper(commandIdentifier, configurationService);
            var retryWrapper = new HystrixRetryWrapper(commandIdentifier, configurationService);
            var circuitBreaker = new HystrixCircuitBreaker(commandIdentifier, configurationService, commandMetrics);
            var threadPoolMetrics = new HystrixThreadPoolMetrics(commandIdentifier, configurationService);

            return new HystrixCommand(commandIdentifier, timeoutWrapper, retryWrapper, circuitBreaker, commandMetrics, threadPoolMetrics, configurationService);
        }
   
        public ICollection<IHystrixCommand> GetAllHystrixCommands()
        {
            return commandsDictionary.Values.Where(x => x.ConfigurationService.GetHystrixCommandEnabled()).ToList();
        }

        /// <summary>
        /// Empty the dictionary, for testing purposes; this method is not exposed in the interface
        /// </summary>
        public static void Clear()
        {
            commandsDictionary.Clear();
        }
    }
}