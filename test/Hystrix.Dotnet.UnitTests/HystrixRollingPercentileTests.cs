using System;
using Hystrix.Dotnet.ConcurrencyUtilities;
using Moq;
using Xunit;

namespace Hystrix.Dotnet.UnitTests
{
    public class HystrixRollingPercentileTests
    {
        public class Constructor
        {
            [Fact]
            public void Throws_ArgumentNullException_When_DateTimeProvider_Is_Null()
            {
                var timeInMilliseconds = 1000;
                var numberOfBuckets = 10;
                var bucketDataLength = 100;
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();

                // Act
                Assert.Throws<ArgumentNullException>(() => new HystrixRollingPercentile(null, timeInMilliseconds, numberOfBuckets, bucketDataLength, configurationServiceMock.Object));
            }

            [Fact]
            public void Throws_ArgumentOutOfRangeException_When_TimeInMilliseconds_Is_Zero_Or_Less()
            {
                var dateTimeProvider = new Mock<IDateTimeProvider>();
                var timeInMilliseconds = 0;
                var numberOfBuckets = 10;
                var bucketDataLength = 100;
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();

                // Act
                Assert.Throws<ArgumentOutOfRangeException>(() => new HystrixRollingPercentile(dateTimeProvider.Object, timeInMilliseconds, numberOfBuckets, bucketDataLength, configurationServiceMock.Object));
            }

            [Fact]
            public void Throws_ArgumentOutOfRangeException_When_NumberOfBuckets_Is_Zero_Or_Less()
            {
                var dateTimeProvider = new Mock<IDateTimeProvider>();
                var timeInMilliseconds = 10000;
                var numberOfBuckets = 0;
                var bucketDataLength = 100;
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();

                // Act
                Assert.Throws<ArgumentOutOfRangeException>(() => new HystrixRollingPercentile(dateTimeProvider.Object, timeInMilliseconds, numberOfBuckets, bucketDataLength, configurationServiceMock.Object));
            }

            [Fact]
            public void Throws_ArgumentOutOfRangeException_When_BucketDataLength_Is_Less_Than_One_Hundred()
            {
                var dateTimeProvider = new Mock<IDateTimeProvider>();
                var timeInMilliseconds = 10000;
                var numberOfBuckets = 10;
                var bucketDataLength = 99;
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();

                // Act
                Assert.Throws<ArgumentOutOfRangeException>(() => new HystrixRollingPercentile(dateTimeProvider.Object, timeInMilliseconds, numberOfBuckets, bucketDataLength, configurationServiceMock.Object));
            }

            [Fact]
            public void Throws_ArgumentNullException_When_ConfigurationService_Is_Null()
            {
                var dateTimeProvider = new Mock<IDateTimeProvider>();
                var timeInMilliseconds = 10000;
                var numberOfBuckets = 0;
                var bucketDataLength = 100;

                // Act
                Assert.Throws<ArgumentOutOfRangeException>(() => new HystrixRollingPercentile(dateTimeProvider.Object, timeInMilliseconds, numberOfBuckets, bucketDataLength, null));
            }

            [Fact]
            public void Throws_ArgumentOutOfRangeException_When_TimeInMilliseconds_Is_Not_An_Exact_Multiple_Of_NumberOfBuckets()
            {
                var dateTimeProvider = new Mock<IDateTimeProvider>();
                var timeInMilliseconds = 10000;
                var numberOfBuckets = 7;
                var bucketDataLength = 100;
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();

                // Act
                Assert.Throws<ArgumentOutOfRangeException>(() => new HystrixRollingPercentile(dateTimeProvider.Object, timeInMilliseconds, numberOfBuckets, bucketDataLength, configurationServiceMock.Object));
            }
        }

        public class AddValue
        {
            [Fact]
            public void Adds_Value_To_Current_Bucket_When_GetMetricsRollingPercentileEnabled_Is_True()
            {
                var dateTimeProvider = new Mock<IDateTimeProvider>();
                var timeInMilliseconds = 10000;
                var numberOfBuckets = 10;
                var bucketDataLength = 100;
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();
                configurationServiceMock.Setup(x => x.GetMetricsRollingPercentileEnabled()).Returns(true);

                var rollingPercentile = new HystrixRollingPercentile(dateTimeProvider.Object, timeInMilliseconds, numberOfBuckets, bucketDataLength, configurationServiceMock.Object);

                // Act
                rollingPercentile.AddValue(243);
            }
        }

        public class GetMean
        {
            [Fact]
            public void Returns_The_Mean_Of_All_Values_In_The_Snapshot_When_GetMetricsRollingPercentileEnabled_Is_True()
            {
                var dateTimeProvider = new Mock<IDateTimeProvider>();
                var timeInMilliseconds = 10000;
                var numberOfBuckets = 10;
                var bucketDataLength = 100;
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();
                configurationServiceMock.Setup(x => x.GetMetricsRollingPercentileEnabled()).Returns(true);

                dateTimeProvider.SetupGet(d => d.CurrentTimeInMilliseconds).Returns(0);

                var rollingPercentile = new HystrixRollingPercentile(dateTimeProvider.Object, timeInMilliseconds, numberOfBuckets, bucketDataLength, configurationServiceMock.Object);
                rollingPercentile.AddValue(243);
                rollingPercentile.AddValue(157);

                dateTimeProvider.SetupGet(d => d.CurrentTimeInMilliseconds).Returns(1500);

                // Act
                var mean = rollingPercentile.GetMean();

                Assert.Equal(200, mean);
            }

            [Fact]
            public void Test_AtomicInteger()
            {
                AtomicInteger value = new AtomicInteger();

                Assert.Equal(0, value.GetValue());

                var andIncrement = value.GetAndIncrement();
                Assert.Equal(0, andIncrement);
                Assert.Equal(1, value.GetValue());
            }

            [Fact]
            public void TestRolling()
            {
                var dateTimeProviderMock = new Mock<IDateTimeProvider>();
                var currentTime = new DateTime(2017, 6, 26, 14, 0, 0).Ticks / TimeSpan.TicksPerMillisecond;
                dateTimeProviderMock.Setup(time => time.CurrentTimeInMilliseconds).Returns(currentTime);

                var configurationServiceMock = new Mock<IHystrixConfigurationService>();
                configurationServiceMock.Setup(x => x.GetMetricsRollingPercentileEnabled()).Returns(true);
                HystrixRollingPercentile p = new HystrixRollingPercentile(dateTimeProviderMock.Object, 60000, 12, 1000, configurationServiceMock.Object);
                p.AddValue(1000);
                p.AddValue(1000);
                p.AddValue(1000);
                p.AddValue(2000);

                Assert.Equal(1, p.Buckets.Length);

                // no bucket turnover yet so percentile not yet generated
                Assert.Equal(0, p.GetPercentile(50));

                currentTime += 6000;
                dateTimeProviderMock.Setup(time => time.CurrentTimeInMilliseconds).Returns(currentTime);

                // still only 1 bucket until we touch it again
                Assert.Equal(1, p.Buckets.Length);

                // a bucket has been created so we have a new percentile
                Assert.Equal(1000, p.GetPercentile(50));

                // now 2 buckets since getting a percentile causes bucket retrieval
                Assert.Equal(2, p.Buckets.Length);

                p.AddValue(1000);
                p.AddValue(500);

                // should still be 2 buckets
                Assert.Equal(2, p.Buckets.Length);

                p.AddValue(200);
                p.AddValue(200);
                p.AddValue(1600);
                p.AddValue(200);
                p.AddValue(1600);
                p.AddValue(1600);

                // we haven't progressed to a new bucket so the percentile should be the same and ignore the most recent bucket
                Assert.Equal(1000, p.GetPercentile(50));

                // increment to another bucket so we include all of the above in the PercentileSnapshot
                currentTime += 6000;
                dateTimeProviderMock.Setup(time => time.CurrentTimeInMilliseconds).Returns(currentTime);

                // the rolling version should have the same data as creating a snapshot like this
                PercentileSnapshot ps = new PercentileSnapshot(1000, 1000, 1000, 2000, 1000, 500, 200, 200, 1600, 200, 1600, 1600);

                Assert.Equal(ps.GetPercentile(0.15), p.GetPercentile(0.15));
                Assert.Equal(ps.GetPercentile(0.50), p.GetPercentile(0.50));
                Assert.Equal(ps.GetPercentile(0.90), p.GetPercentile(0.90));
                Assert.Equal(ps.GetPercentile(0.995), p.GetPercentile(0.995));

                // mean = 1000+1000+1000+2000+1000+500+200+200+1600+200+1600+1600/12
                Assert.Equal(991, ps.GetMean());
            }
        }
    }
}
