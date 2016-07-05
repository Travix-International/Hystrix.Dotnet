using System;
using System.Linq;
using Xunit;

namespace Hystrix.Dotnet.UnitTests
{
    public class RollingNumberBucketTests
    {
        public class Get
        {
            [Fact]
            public void Returns_Zero_For_All_HystrixRollingNumberEvent_Items()
            {
                var bucket = new RollingNumberBucket(DateTime.UtcNow.Ticks/TimeSpan.TicksPerMillisecond);
                var enumValues = Enum.GetValues(typeof(HystrixRollingNumberEvent)).Cast<HystrixRollingNumberEvent>();

                foreach (var enumValue in enumValues)
                {
                    long value = bucket.Get(enumValue);

                    Assert.Equal(0L, value);
                }
            }
        }

        public class GetAdder
        {
            [Fact]
            public void Returns_StripedLongAdder_For_All_Hystrix_RollingNumberEvent_Items_That_Are_Counters()
            {
                var bucket = new RollingNumberBucket(DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond);
                var enumValues = Enum.GetValues(typeof(HystrixRollingNumberEvent)).Cast<HystrixRollingNumberEvent>();

                foreach (var enumValue in enumValues)
                {
                    if (enumValue.IsCounter())
                    {
                        var adder = bucket.GetAdder(enumValue);

                        Assert.NotNull(adder);
                    }
                }
            }

            [Fact]
            public void Throws_InvalidOperationException_For_All_Hystrix_RollingNumberEvent_Items_That_Are_Not_Counters()
            {
                var bucket = new RollingNumberBucket(DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond);
                var enumValues = Enum.GetValues(typeof(HystrixRollingNumberEvent)).Cast<HystrixRollingNumberEvent>();

                foreach (var enumValue in enumValues)
                {
                    if (!enumValue.IsCounter())
                    {
                        HystrixRollingNumberEvent value = enumValue;
                        Assert.Throws<InvalidOperationException>(() => bucket.GetAdder(value));
                    }
                }
            }
        }

        public class GetMaxUpdater
        {
            [Fact]
            public void Returns_LongMaxUpdater_For_All_Hystrix_RollingNumberEvent_Items_That_Are_MaxUpdaters()
            {
                var bucket = new RollingNumberBucket(DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond);
                var enumValues = Enum.GetValues(typeof(HystrixRollingNumberEvent)).Cast<HystrixRollingNumberEvent>();

                foreach (var enumValue in enumValues)
                {
                    if (enumValue.IsMaxUpdater())
                    {
                        var adder = bucket.GetMaxUpdater(enumValue);

                        Assert.NotNull(adder);
                    }
                }
            }

            [Fact]
            public void Throws_InvalidOperationException_For_All_Hystrix_RollingNumberEvent_Items_That_Are_Not_MaxUpdaters()
            {
                var bucket = new RollingNumberBucket(DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond);
                var enumValues = Enum.GetValues(typeof(HystrixRollingNumberEvent)).Cast<HystrixRollingNumberEvent>();

                foreach (var enumValue in enumValues)
                {
                    if (!enumValue.IsMaxUpdater())
                    {
                        HystrixRollingNumberEvent value = enumValue;
                        Assert.Throws<InvalidOperationException>(() => bucket.GetMaxUpdater(value));
                    }
                }
            }
        }
    }
}
