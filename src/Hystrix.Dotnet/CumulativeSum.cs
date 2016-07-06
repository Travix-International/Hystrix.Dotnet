using System;
using System.Linq;
using Hystrix.Dotnet.ConcurrencyUtilities;

namespace Hystrix.Dotnet
{
    internal class CumulativeSum
    {
        private readonly StripedLongAdder[] adderForCounterType;
        private readonly LongMaxUpdater[] updaterForCounterType;

        public CumulativeSum() 
        {
            /*
             * We support both LongAdder and LongMaxUpdater in a bucket but don't want the memory allocation
             * of all types for each so we only allocate the objects if the HystrixRollingNumberEvent matches
             * the correct type - though we still have the allocation of empty arrays to the given length
             * as we want to keep using the (int)type value for fast random access.
             */

            // initialize the array of LongAdders
            var values = Enum.GetValues(typeof(HystrixRollingNumberEvent)).Cast<HystrixRollingNumberEvent>();

            adderForCounterType = new StripedLongAdder[values.Count()];
            foreach (var value in values)
            {
                if (value.IsCounter())
                {
                    adderForCounterType[(int)value] = new StripedLongAdder();
                }
            }

            updaterForCounterType = new LongMaxUpdater[values.Count()];
            foreach (var value in values)
            {
                if (value.IsMaxUpdater())
                {
                    updaterForCounterType[(int)value] = new LongMaxUpdater();
                    // initialize to 0 otherwise it is Long.MIN_VALUE
                    updaterForCounterType[(int)value].Update(0);
                }
            }
        }

        public void AddBucket(RollingNumberBucket lastBucket) 
        {
            var values = Enum.GetValues(typeof(HystrixRollingNumberEvent)).Cast<HystrixRollingNumberEvent>();
            foreach (var value in values)
            {
                if (value.IsCounter()) 
                {
                    GetAdder(value).Add(lastBucket.GetAdder(value).GetValue());
                }
                if (value.IsMaxUpdater()) 
                {
                    GetMaxUpdater(value).Update(lastBucket.GetMaxUpdater(value).Max());
                }
            }
        }

        public long Get(HystrixRollingNumberEvent type) 
        {
            if (type.IsCounter()) 
            {
                return adderForCounterType[(int)type].GetValue();
            }
            if (type.IsMaxUpdater()) 
            {
                return updaterForCounterType[(int)type].Max();
            }

            throw new InvalidOperationException("Unknown type of event: " + type);
        }

        public StripedLongAdder GetAdder(HystrixRollingNumberEvent type)
        {
            if (!type.IsCounter()) 
            {
                throw new InvalidOperationException("Type is not a Counter: " + type);
            }

            return adderForCounterType[(int)type];
        }

        public LongMaxUpdater GetMaxUpdater(HystrixRollingNumberEvent type)
        {
            if (!type.IsMaxUpdater()) 
            {
                throw new InvalidOperationException("Type is not a MaxUpdater: " + type);
            }
            return updaterForCounterType[(int)type];
        }
    }
}
