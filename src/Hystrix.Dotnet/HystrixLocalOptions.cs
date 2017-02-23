using System.Collections.Generic;

namespace Hystrix.Dotnet
{
    public class HystrixLocalOptions
    {
        public HystrixLocalOptions()
        {
            CommandOptions = new Dictionary<string, Dictionary<string, HystrixCommandOptions>>();
        }

        public static HystrixLocalOptions CreateDefault()
        {
            return new HystrixLocalOptions();
        }

        public Dictionary<string, Dictionary<string, HystrixCommandOptions>> CommandOptions { get; set; }

        public HystrixCommandOptions GetCommandOptions(HystrixCommandIdentifier id) => GetCommandOptions(id.GroupKey, id.CommandKey);

        public HystrixCommandOptions GetCommandOptions(string groupKey, string commandKey)
        {
            if (CommandOptions == null)
            {
                return HystrixCommandOptions.CreateDefault();
            }

            Dictionary<string, HystrixCommandOptions> groupCommands;

            if (!CommandOptions.TryGetValue(groupKey, out groupCommands))
            {
                return HystrixCommandOptions.CreateDefault();
            }

            HystrixCommandOptions commandOptions;

            if (!groupCommands.TryGetValue(commandKey, out commandOptions))
            {
                return HystrixCommandOptions.CreateDefault();
            }

            return commandOptions;
        }
    }
}
