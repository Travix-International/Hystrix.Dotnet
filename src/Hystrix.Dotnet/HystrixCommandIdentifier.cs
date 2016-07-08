using System;

namespace Hystrix.Dotnet
{
    public class HystrixCommandIdentifier
    {
        public string GroupKey { get; private set; }
        public string CommandKey { get; private set; }

        public HystrixCommandIdentifier(string groupKey, string commandKey)
        {
            if (groupKey == null)
            {
                throw new ArgumentNullException("groupKey");
            }
            if (string.Empty.Equals(groupKey, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("groupKey");
            }
            if (commandKey == null)
            {
                throw new ArgumentNullException("commandKey");
            }
            if (string.Empty.Equals(commandKey, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("commandKey");
            }

            GroupKey = groupKey;
            CommandKey = commandKey;
        }

        public override int GetHashCode()
        {
            // http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode/263416#263416
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;

                // Suitable nullity checks etc, of course :)
                hash = hash * 486187739 + GroupKey.ToLowerInvariant().GetHashCode();
                hash = hash * 486187739 + CommandKey.ToLowerInvariant().GetHashCode();

                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as HystrixCommandIdentifier);
        }

        public bool Equals(HystrixCommandIdentifier obj)
        {
            return obj != null && obj.GroupKey.Equals(GroupKey, StringComparison.OrdinalIgnoreCase) && obj.CommandKey.Equals(CommandKey, StringComparison.OrdinalIgnoreCase);
        }
    }
}