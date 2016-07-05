using System;
using System.Threading;
using Moq;
using Xunit;

namespace Hystrix.Dotnet.UnitTests
{
    public class HystrixRollingPercentileTests
    {
        public class Constructor
        {
            [Fact]
            public void Throws_ArgumentOutOfRangeException_When_TimeInMilliseconds_Is_Zero_Or_Less()
            {
                var timeInMilliseconds = 0;
                var numberOfBuckets = 10;
                var bucketDataLength = 100;
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();

                // act
                Assert.Throws<ArgumentOutOfRangeException>(() => new HystrixRollingPercentile(timeInMilliseconds, numberOfBuckets, bucketDataLength, configurationServiceMock.Object));
            }

            [Fact]
            public void Throws_ArgumentOutOfRangeException_When_NumberOfBuckets_Is_Zero_Or_Less()
            {
                var timeInMilliseconds = 10000;
                var numberOfBuckets = 0;
                var bucketDataLength = 100;
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();

                // act
                Assert.Throws<ArgumentOutOfRangeException>(() => new HystrixRollingPercentile(timeInMilliseconds, numberOfBuckets, bucketDataLength, configurationServiceMock.Object));
            }

            [Fact]
            public void Throws_ArgumentOutOfRangeException_When_BucketDataLength_Is_Less_Than_One_Hundred()
            {
                var timeInMilliseconds = 10000;
                var numberOfBuckets = 10;
                var bucketDataLength = 99;
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();

                // act
                Assert.Throws<ArgumentOutOfRangeException>(() => new HystrixRollingPercentile(timeInMilliseconds, numberOfBuckets, bucketDataLength, configurationServiceMock.Object));
            }

            [Fact]
            public void Throws_ArgumentNullException_When_ConfigurationService_Is_Null()
            {
                var timeInMilliseconds = 10000;
                var numberOfBuckets = 0;
                var bucketDataLength = 100;

                // act
                Assert.Throws<ArgumentOutOfRangeException>(() => new HystrixRollingPercentile(timeInMilliseconds, numberOfBuckets, bucketDataLength, null));
            }

            [Fact]
            public void Throws_ArgumentOutOfRangeException_When_TimeInMilliseconds_Is_Not_An_Exact_Multiple_Of_NumberOfBuckets()
            {
                var timeInMilliseconds = 10000;
                var numberOfBuckets = 7;
                var bucketDataLength = 100;
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();

                // act
                Assert.Throws<ArgumentOutOfRangeException>(() => new HystrixRollingPercentile(timeInMilliseconds, numberOfBuckets, bucketDataLength, configurationServiceMock.Object));
            }
        }

        public class AddValue
        {
            [Fact]
            public void Adds_Value_To_Current_Bucket_When_GetMetricsRollingPercentileEnabled_Is_True()
            {
                var timeInMilliseconds = 10000;
                var numberOfBuckets = 10;
                var bucketDataLength = 100;
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();
                configurationServiceMock.Setup(x => x.GetMetricsRollingPercentileEnabled()).Returns(true);
                var rollingPercentile = new HystrixRollingPercentile(timeInMilliseconds, numberOfBuckets, bucketDataLength, configurationServiceMock.Object);

                // act
                rollingPercentile.AddValue(243);
            }
        }

        public class GetMean
        {
            [Fact]
            public void Returns_The_Mean_Of_All_Values_In_The_Snapshot_When_GetMetricsRollingPercentileEnabled_Is_True()
            {
                var timeInMilliseconds = 10000;
                var numberOfBuckets = 10;
                var bucketDataLength = 100;
                var configurationServiceMock = new Mock<IHystrixConfigurationService>();
                configurationServiceMock.Setup(x => x.GetMetricsRollingPercentileEnabled()).Returns(true);
                var rollingPercentile = new HystrixRollingPercentile(timeInMilliseconds, numberOfBuckets, bucketDataLength, configurationServiceMock.Object);
                rollingPercentile.AddValue(243);
                rollingPercentile.AddValue(157);

                Thread.Sleep(1500);

                // act
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
                //MockedTime time = new MockedTime();
                var dateTimeProviderMock = new Mock<DateTimeProvider>();
                var currentTime = new DateTimeProvider().GetCurrentTimeInMilliseconds();
                dateTimeProviderMock.Setup(time => time.GetCurrentTimeInMilliseconds()).Returns(currentTime);

                //HystrixRollingPercentile p = new HystrixRollingPercentile(time, 60000, 12, 1000, true);
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
                dateTimeProviderMock.Setup(time => time.GetCurrentTimeInMilliseconds()).Returns(currentTime);

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
                dateTimeProviderMock.Setup(time => time.GetCurrentTimeInMilliseconds()).Returns(currentTime);

                // the rolling version should have the same data as creating a snapshot like this
                PercentileSnapshot ps = new PercentileSnapshot(1000, 1000, 1000, 2000, 1000, 500, 200, 200, 1600, 200, 1600, 1600);

                Assert.Equal(ps.GetPercentile(0.15), p.GetPercentile(0.15));
                Assert.Equal(ps.GetPercentile(0.50), p.GetPercentile(0.50));
                Assert.Equal(ps.GetPercentile(0.90), p.GetPercentile(0.90));
                Assert.Equal(ps.GetPercentile(0.995), p.GetPercentile(0.995));

                //System.out.println("100th: " + ps.GetPercentile(100) + "  " + p.GetPercentile(100));
                //System.out.println("99.5th: " + ps.GetPercentile(99.5) + "  " + p.GetPercentile(99.5));
                //System.out.println("99th: " + ps.GetPercentile(99) + "  " + p.GetPercentile(99));
                //System.out.println("90th: " + ps.GetPercentile(90) + "  " + p.GetPercentile(90));
                //System.out.println("50th: " + ps.GetPercentile(50) + "  " + p.GetPercentile(50));
                //System.out.println("10th: " + ps.GetPercentile(10) + "  " + p.GetPercentile(10));

                // mean = 1000+1000+1000+2000+1000+500+200+200+1600+200+1600+1600/12
                Assert.Equal(991, ps.GetMean());
            }
        }
    }
}
