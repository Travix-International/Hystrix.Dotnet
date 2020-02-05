using System.Collections.Generic;

namespace Hystrix.Dotnet
{
    public class HystrixLocalOptions
    {
        public HystrixLocalOptions()
        {
            CommandGroups = new Dictionary<string, Dictionary<string, HystrixCommandOptions>>();
        }

        public static HystrixLocalOptions CreateDefault()
        {
            return new HystrixLocalOptions();
        }

        public Dictionary<string, Dictionary<string, HystrixCommandOptions>> CommandGroups { get; set; }

        public HystrixCommandOptions DefaultOptions { get; set; }

        public HystrixCommandOptions GetCommandOptions(HystrixCommandIdentifier id) => GetCommandOptions(id.GroupKey, id.CommandKey);

        public HystrixCommandOptions GetCommandOptions(string groupKey, string commandKey)
        {
            if (CommandGroups == null)
            {
                return DefaultOptions ?? HystrixCommandOptions.CreateDefault();
            }

            if (!CommandGroups.TryGetValue(groupKey, out var groupCommands))
            {
                return DefaultOptions ?? HystrixCommandOptions.CreateDefault();
            }

            if (!groupCommands.TryGetValue(commandKey, out var commandOptions))
            {
                return DefaultOptions ?? HystrixCommandOptions.CreateDefault();
            }

            return commandOptions;
        }
    }
}