using System;
using Moq;
using Xunit;

namespace Hystrix.Dotnet.UnitTests
{
    public class HystrixRollingNumberTests
    {
        public class Constructor
        {
            [Fact]
            public void Throws_ArgumentOutOfRangeException_When_DateTimeProvider_Is_Null()
            {
                var dateTimeProvider = new Mock<IDateTimeProvider>();
                var timeInMilliseconds = 0;
                var numberOfBuckets = 10;

                // Act
                Assert.Throws<ArgumentOutOfRangeException>(() => new HystrixRollingNumber(dateTimeProvider.Object, timeInMilliseconds, numberOfBuckets));
            }

            [Fact]
            public void Throws_ArgumentOutOfRangeException_When_TimeInMilliseconds_Is_Zero_Or_Less()
            {
                var dateTimeProvider = new Mock<IDateTimeProvider>();
                var timeInMilliseconds = 0;
                var numberOfBuckets = 10;

                // Act
                Assert.Throws<ArgumentOutOfRangeException>(() => new HystrixRollingNumber(dateTimeProvider.Object, timeInMilliseconds, numberOfBuckets));
            }

            [Fact]
            public void Throws_ArgumentOutOfRangeException_When_NumberOfBuckets_Is_Zero_Or_Less()
            {
                var dateTimeProvider = new Mock<IDateTimeProvider>();
                var timeInMilliseconds = 10000;
                var numberOfBuckets = 0;

                // Act
                Assert.Throws<ArgumentOutOfRangeException>(() => new HystrixRollingNumber(dateTimeProvider.Object, timeInMilliseconds, numberOfBuckets));
            }

            [Fact]
            public void Throws_ArgumentOutOfRangeException_When_TimeInMilliseconds_Is_Not_A_Multitude_Of_NumberOfBuckets()
            {
                var dateTimeProvider = new Mock<IDateTimeProvider>();
                var timeInMilliseconds = 10000;
                var numberOfBuckets = 7;

                // Act
                Assert.Throws<ArgumentOutOfRangeException>(() => new HystrixRollingNumber(dateTimeProvider.Object, timeInMilliseconds, numberOfBuckets));
            }

            [Fact]
            public void Sets_TimeInMilliseconds_And_NumberOfBuckets()
            {
                var dateTimeProvider = new Mock<IDateTimeProvider>();
                var timeInMilliseconds = 10000;
                var numberOfBuckets = 10;

                // Act
                var rollingNumber = new HystrixRollingNumber(dateTimeProvider.Object, timeInMilliseconds, numberOfBuckets);

                Assert.Equal(10000, rollingNumber.TimeInMilliseconds);
                Assert.Equal(10, rollingNumber.NumberOfBuckets);
            }

            [Fact]
            public void Calculates_BucketSizeInMillseconds_From_TimeInMilliseconds_Divided_By_NumberOfBuckets()
            {
                var dateTimeProvider = new Mock<IDateTimeProvider>();
                var timeInMilliseconds = 10000;
                var numberOfBuckets = 10;

                // Act
                var rollingNumber = new HystrixRollingNumber(dateTimeProvider.Object, timeInMilliseconds, numberOfBuckets);

                Assert.Equal(1000, rollingNumber.BucketSizeInMillseconds);
            }
        }


        public class GetValueOfLatestBucket
        {
            [Fact]
            public void Returns_Zero_If_No_Values_Have_Been_Added_Yet_For_Specific_Counter()
            {
                var dateTimeProvider = new Mock<IDateTimeProvider>();
                var rollingNumber = new HystrixRollingNumber(dateTimeProvider.Object, 10000, 10);

                // Act
                long valueOfLatestBucket = rollingNumber.GetValueOfLatestBucket(HystrixRollingNumberEvent.Success);

                Assert.Equal(0L, valueOfLatestBucket);
            }
        }

        public class Increment
        {
            [Fact]
            public void Increments_The_Counter()
            {
                var dateTimeProvider = new Mock<IDateTimeProvider>();
                var rollingNumber = new HystrixRollingNumber(dateTimeProvider.Object, 10000, 10);

                // Act
                rollingNumber.Increment(HystrixRollingNumberEvent.Success);

                long valueOfLatestBucket = rollingNumber.GetValueOfLatestBucket(HystrixRollingNumberEvent.Success);
                Assert.Equal(1L, valueOfLatestBucket);
            }
        }

        public class Add
        {
            [Fact]
            public void Adds_The_Value_To_The_Counter()
            {
                var dateTimeProvider = new Mock<IDateTimeProvider>();
                long value = 15;
                var rollingNumber = new HystrixRollingNumber(dateTimeProvider.Object, 10000, 10);

                // Act
                rollingNumber.Add(HystrixRollingNumberEvent.Success, value);

                long valueOfLatestBucket = rollingNumber.GetValueOfLatestBucket(HystrixRollingNumberEvent.Success);
                Assert.Equal(15L, valueOfLatestBucket);
            }
        }

        public class UpdateRollingMax
        {
            [Fact]
            public void Sets_The_Max_Of_Current_Value_And_Passed_Value()
            {
                var dateTimeProvider = new Mock<IDateTimeProvider>();
                long value = 15;
                var rollingNumber = new HystrixRollingNumber(dateTimeProvider.Object, 10000, 10);

                // Act
                rollingNumber.UpdateRollingMax(HystrixRollingNumberEvent.CommandMaxActive, value);

                long valueOfLatestBucket = rollingNumber.GetValueOfLatestBucket(HystrixRollingNumberEvent.CommandMaxActive);
                Assert.Equal(15L, valueOfLatestBucket);
            }            
        }

        public class Reset
        {
            [Fact]
            public void Clears_All_Buckets()
            {
                var dateTimeProvider = new Mock<IDateTimeProvider>();
                var rollingNumber = new HystrixRollingNumber(dateTimeProvider.Object, 10000, 10);
                rollingNumber.Increment(HystrixRollingNumberEvent.Success);

                long rollingSumBeforeReset = rollingNumber.GetRollingSum(HystrixRollingNumberEvent.Success);
                Assert.Equal(1L, rollingSumBeforeReset);

                // Act
                rollingNumber.Reset();

                long rollingSumAfterReset = rollingNumber.GetRollingSum(HystrixRollingNumberEvent.Success);
                Assert.Equal(0L, rollingSumAfterReset);
            }
        }

        public class GetCumulativeSum
        {
            [Fact]
            public void NotImplemented()
            {
                var dateTimeProvider = new Mock<IDateTimeProvider>();
                var rollingNumber = new HystrixRollingNumber(dateTimeProvider.Object, 10000, 10);

                // Act
                long cumulativeSum = rollingNumber.GetCumulativeSum(HystrixRollingNumberEvent.Success);

                Assert.Equal(0L, cumulativeSum);
            }
        }

        public class GetRollingSum
        {
            [Fact]
            public void NotImplemented()
            {
                var dateTimeProvider = new Mock<IDateTimeProvider>();
                var rollingNumber = new HystrixRollingNumber(dateTimeProvider.Object, 10000, 10);

                // Act
                long value =  rollingNumber.GetRollingSum(HystrixRollingNumberEvent.Success);

                Assert.Equal(0, value);
            }
        }

        public class GetValues
        {
            [Fact]
            public void Returns_Values_For_All_Buckets()
            {
                var dateTimeProvider = new Mock<IDateTimeProvider>();
                var rollingNumber = new HystrixRollingNumber(dateTimeProvider.Object, 10000, 10);

                // Act
                long[] values = rollingNumber.GetValues(HystrixRollingNumberEvent.Success);

                Assert.Equal(1, values.Length);
            }
        }

        public class GetRollingMaxValue
        {
            [Fact]
            public void Returns_Zero_If_No_Values_Have_Been_Recorded()
            {
                var dateTimeProvider = new Mock<IDateTimeProvider>();
                var rollingNumber = new HystrixRollingNumber(dateTimeProvider.Object, 10000, 10);

                // Act
                long rollingMaxValue = rollingNumber.GetRollingMaxValue(HystrixRollingNumberEvent.Success);

                Assert.Equal(0L, rollingMaxValue);
            }
        }
    }
}
