using System;
using Xunit;

namespace Hystrix.Dotnet.UnitTests
{
    public class HystrixWebConfigConfigurationServiceTests
    {
        public class Constructor
        {
            [Fact]
            public void Throws_ArgumentNullException_When_CommandIdentifier_Is_Null()
            {
                // act
                Assert.Throws<ArgumentNullException>(() => new HystrixWebConfigConfigurationService(null));
            }
        }

        public class GetCommandTimeoutInMilliseconds
        {
            [Fact]
            public void Returns_CommandTimeoutInMilliseconds_AppSetting_As_Integer_For_GroupA_And_DependencyX()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupA", "DependencyX");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                int value = hystrixConfigurationService.GetCommandTimeoutInMilliseconds();

                Assert.Equal(1500, value);
            }

            [Fact]
            public void Returns_CommandTimeoutInMilliseconds_AppSetting_As_Integer_For_GroupB_And_DependencyY()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupB", "DependencyY");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                int value = hystrixConfigurationService.GetCommandTimeoutInMilliseconds();

                Assert.Equal(750, value);
            }

            [Fact]
            public void Returns_1000_If_AppSetting_Is_Not_A_Valid_Integer()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupC", "DependencyZ");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                int value = hystrixConfigurationService.GetCommandTimeoutInMilliseconds();

                Assert.Equal(1000, value);
            }

            [Fact]
            public void Returns_1000_If_AppSetting_Does_Not_Exist()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("NonExistingGroup", "NonExistingCommand");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                int value = hystrixConfigurationService.GetCommandTimeoutInMilliseconds();

                Assert.Equal(1000, value);
            }
        }

        public class GetCircuitBreakerForcedOpen
        {
            [Fact]
            public void Returns_GetCircuitBreakerForcedOpen_AppSetting_As_Boolean_For_GroupA_And_DependencyX()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupA", "DependencyX");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                bool value = hystrixConfigurationService.GetCircuitBreakerForcedOpen();

                Assert.Equal(true, value);
            }

            [Fact]
            public void Returns_GetCircuitBreakerForcedOpen_AppSetting_As_Boolean_For_GroupB_And_DependencyY()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupB", "DependencyY");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                bool value = hystrixConfigurationService.GetCircuitBreakerForcedOpen();

                Assert.Equal(false, value);
            }

            [Fact]
            public void Returns_False_If_AppSetting_Is_Not_A_Valid_Boolean()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupC", "DependencyZ");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                bool value = hystrixConfigurationService.GetCircuitBreakerForcedOpen();

                Assert.Equal(false, value);
            }

            [Fact]
            public void Returns_False_If_AppSetting_Does_Not_Exist()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("NonExistingGroup", "NonExistingCommand");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                bool value = hystrixConfigurationService.GetCircuitBreakerForcedOpen();

                Assert.Equal(false, value);
            }
        }

        public class GetCircuitBreakerForcedClosed
        {
            [Fact]
            public void Returns_GetCircuitBreakerForcedClosed_AppSetting_As_Boolean_For_GroupA_And_DependencyX()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupA", "DependencyX");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                bool value = hystrixConfigurationService.GetCircuitBreakerForcedClosed();

                Assert.Equal(true, value);
            }

            [Fact]
            public void Returns_GetCircuitBreakerForcedClosed_AppSetting_As_Boolean_For_GroupB_And_DependencyY()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupB", "DependencyY");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                bool value = hystrixConfigurationService.GetCircuitBreakerForcedClosed();

                Assert.Equal(false, value);
            }

            [Fact]
            public void Returns_False_If_AppSetting_Is_Not_A_Valid_Boolean()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupC", "DependencyZ");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                bool value = hystrixConfigurationService.GetCircuitBreakerForcedClosed();

                Assert.Equal(false, value);
            }

            [Fact]
            public void Returns_False_If_AppSetting_Does_Not_Exist()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("NonExistingGroup", "NonExistingCommand");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                bool value = hystrixConfigurationService.GetCircuitBreakerForcedClosed();

                Assert.Equal(false, value);
            }
        }

        public class GetCircuitBreakerErrorThresholdPercentage
        {
            [Fact]
            public void Returns_CircuitBreakerErrorThresholdPercentage_AppSetting_As_Integer_For_GroupA_And_DependencyX()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupA", "DependencyX");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                int value = hystrixConfigurationService.GetCircuitBreakerErrorThresholdPercentage();

                Assert.Equal(25, value);
            }

            [Fact]
            public void Returns_CircuitBreakerErrorThresholdPercentage_AppSetting_As_Integer_For_GroupB_And_DependencyY()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupB", "DependencyY");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                int value = hystrixConfigurationService.GetCircuitBreakerErrorThresholdPercentage();

                Assert.Equal(15, value);
            }

            [Fact]
            public void Returns_50_If_AppSetting_Is_Not_A_Valid_Integer()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupC", "DependencyZ");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                int value = hystrixConfigurationService.GetCircuitBreakerErrorThresholdPercentage();

                Assert.Equal(50, value);
            }

            [Fact]
            public void Returns_50_If_AppSetting_Does_Not_Exist()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("NonExistingGroup", "NonExistingCommand");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                int value = hystrixConfigurationService.GetCircuitBreakerErrorThresholdPercentage();

                Assert.Equal(50, value);
            }
        }

        public class GetCircuitBreakerSleepWindowInMilliseconds
        {
            [Fact]
            public void Returns_CircuitBreakerSleepWindowInMilliseconds_AppSetting_As_Integer_For_GroupA_And_DependencyX()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupA", "DependencyX");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                int value = hystrixConfigurationService.GetCircuitBreakerSleepWindowInMilliseconds();

                Assert.Equal(6000, value);
            }

            [Fact]
            public void Returns_CircuitBreakerSleepWindowInMilliseconds_AppSetting_As_Integer_For_GroupB_And_DependencyY()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupB", "DependencyY");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                int value = hystrixConfigurationService.GetCircuitBreakerSleepWindowInMilliseconds();

                Assert.Equal(7500, value);
            }

            [Fact]
            public void Returns_5000_If_AppSetting_Is_Not_A_Valid_Integer()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupC", "DependencyZ");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                int value = hystrixConfigurationService.GetCircuitBreakerSleepWindowInMilliseconds();

                Assert.Equal(5000, value);
            }

            [Fact]
            public void Returns_5000_If_AppSetting_Does_Not_Exist()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("NonExistingGroup", "NonExistingCommand");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                int value = hystrixConfigurationService.GetCircuitBreakerSleepWindowInMilliseconds();

                Assert.Equal(5000, value);
            }
        }

        public class GetCircuitBreakerRequestVolumeThreshold
        {
            [Fact]
            public void Returns_CircuitBreakerRequestVolumeThreshold_AppSetting_As_Integer_For_GroupA_And_DependencyX()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupA", "DependencyX");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                int value = hystrixConfigurationService.GetCircuitBreakerRequestVolumeThreshold();

                Assert.Equal(50000, value);
            }

            [Fact]
            public void Returns_CircuitBreakerRequestVolumeThreshold_AppSetting_As_Integer_For_GroupB_And_DependencyY()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupB", "DependencyY");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                int value = hystrixConfigurationService.GetCircuitBreakerRequestVolumeThreshold();

                Assert.Equal(45000, value);
            }

            [Fact]
            public void Returns_20_If_AppSetting_Is_Not_A_Valid_Integer()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupC", "DependencyZ");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                int value = hystrixConfigurationService.GetCircuitBreakerRequestVolumeThreshold();

                Assert.Equal(20, value);
            }

            [Fact]
            public void Returns_20_If_AppSetting_Does_Not_Exist()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("NonExistingGroup", "NonExistingCommand");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                int value = hystrixConfigurationService.GetCircuitBreakerRequestVolumeThreshold();

                Assert.Equal(20, value);
            }
        }

        public class GetMetricsHealthSnapshotIntervalInMilliseconds
        {
            [Fact]
            public void Returns_MetricsHealthSnapshotIntervalInMilliseconds_AppSetting_As_Integer_For_GroupA_And_DependencyX()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupA", "DependencyX");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                int value = hystrixConfigurationService.GetMetricsHealthSnapshotIntervalInMilliseconds();

                Assert.Equal(3000, value);
            }

            [Fact]
            public void Returns_MetricsHealthSnapshotIntervalInMilliseconds_AppSetting_As_Integer_For_GroupB_And_DependencyY()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupB", "DependencyY");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                int value = hystrixConfigurationService.GetMetricsHealthSnapshotIntervalInMilliseconds();

                Assert.Equal(1500, value);
            }

            [Fact]
            public void Returns_500_If_AppSetting_Is_Not_A_Valid_Integer()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupC", "DependencyZ");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                int value = hystrixConfigurationService.GetMetricsHealthSnapshotIntervalInMilliseconds();

                Assert.Equal(500, value);
            }

            [Fact]
            public void Returns_500_If_AppSetting_Does_Not_Exist()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("NonExistingGroup", "NonExistingCommand");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                int value = hystrixConfigurationService.GetMetricsHealthSnapshotIntervalInMilliseconds();

                Assert.Equal(500, value);
            }
        }

        public class GetMetricsRollingStatisticalWindowInMilliseconds
        {
            [Fact]
            public void Returns_MetricsRollingStatisticalWindowInMilliseconds_AppSetting_As_Integer_For_GroupA_And_DependencyX()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupA", "DependencyX");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                int value = hystrixConfigurationService.GetMetricsRollingStatisticalWindowInMilliseconds();

                Assert.Equal(3000, value);
            }

            [Fact]
            public void Returns_MetricsRollingStatisticalWindowInMilliseconds_AppSetting_As_Integer_For_GroupB_And_DependencyY()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupB", "DependencyY");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                int value = hystrixConfigurationService.GetMetricsRollingStatisticalWindowInMilliseconds();

                Assert.Equal(1500, value);
            }

            [Fact]
            public void Returns_10000_If_AppSetting_Is_Not_A_Valid_Integer()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupC", "DependencyZ");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                int value = hystrixConfigurationService.GetMetricsRollingStatisticalWindowInMilliseconds();

                Assert.Equal(10000, value);
            }

            [Fact]
            public void Returns_10000_If_AppSetting_Does_Not_Exist()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("NonExistingGroup", "NonExistingCommand");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                int value = hystrixConfigurationService.GetMetricsRollingStatisticalWindowInMilliseconds();

                Assert.Equal(10000, value);
            }
        }

        public class GetMetricsRollingStatisticalWindowBuckets
        {
            [Fact]
            public void Returns_MetricsRollingStatisticalWindowBuckets_AppSetting_As_Integer_For_GroupA_And_DependencyX()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupA", "DependencyX");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                int value = hystrixConfigurationService.GetMetricsRollingStatisticalWindowBuckets();

                Assert.Equal(3000, value);
            }

            [Fact]
            public void Returns_MetricsRollingStatisticalWindowBuckets_AppSetting_As_Integer_For_GroupB_And_DependencyY()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupB", "DependencyY");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                int value = hystrixConfigurationService.GetMetricsRollingStatisticalWindowBuckets();

                Assert.Equal(1500, value);
            }

            [Fact]
            public void Returns_10_If_AppSetting_Is_Not_A_Valid_Integer()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupC", "DependencyZ");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                int value = hystrixConfigurationService.GetMetricsRollingStatisticalWindowBuckets();

                Assert.Equal(10, value);
            }

            [Fact]
            public void Returns_10_If_AppSetting_Does_Not_Exist()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("NonExistingGroup", "NonExistingCommand");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                int value = hystrixConfigurationService.GetMetricsRollingStatisticalWindowBuckets();

                Assert.Equal(10, value);
            }
        }

        public class GetMetricsRollingPercentileEnabled
        {
            [Fact]
            public void Returns_MetricsRollingPercentileEnabled_AppSetting_As_Boolean_For_GroupA_And_DependencyX()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupA", "DependencyX");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                bool value = hystrixConfigurationService.GetMetricsRollingPercentileEnabled();

                Assert.True(value);
            }

            [Fact]
            public void Returns_MetricsRollingPercentileEnabled_AppSetting_As_Boolean_For_GroupB_And_DependencyY()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupB", "DependencyY");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                bool value = hystrixConfigurationService.GetMetricsRollingPercentileEnabled();

                Assert.False(value);
            }

            [Fact]
            public void Returns_True_If_AppSetting_Is_Not_A_Valid_Integer()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupC", "DependencyZ");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                bool value = hystrixConfigurationService.GetMetricsRollingPercentileEnabled();

                Assert.True(value);
            }

            [Fact]
            public void Returns_True_If_AppSetting_Does_Not_Exist()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("NonExistingGroup", "NonExistingCommand");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                bool value = hystrixConfigurationService.GetMetricsRollingPercentileEnabled();

                Assert.True(value);
            }
        }

        public class GetMetricsRollingPercentileWindowInMilliseconds
        {
            [Fact]
            public void Returns_MetricsRollingPercentileWindowInMilliseconds_AppSetting_As_Integer_For_GroupA_And_DependencyX()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupA", "DependencyX");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                int value = hystrixConfigurationService.GetMetricsRollingPercentileWindowInMilliseconds();

                Assert.Equal(3000, value);
            }

            [Fact]
            public void Returns_MetricsRollingPercentileWindowInMilliseconds_AppSetting_As_Integer_For_GroupB_And_DependencyY()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupB", "DependencyY");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                int value = hystrixConfigurationService.GetMetricsRollingPercentileWindowInMilliseconds();

                Assert.Equal(1500, value);
            }

            [Fact]
            public void Returns_60000_If_AppSetting_Is_Not_A_Valid_Integer()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupC", "DependencyZ");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                int value = hystrixConfigurationService.GetMetricsRollingPercentileWindowInMilliseconds();

                Assert.Equal(60000, value);
            }

            [Fact]
            public void Returns_60000_If_AppSetting_Does_Not_Exist()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("NonExistingGroup", "NonExistingCommand");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                int value = hystrixConfigurationService.GetMetricsRollingPercentileWindowInMilliseconds();

                Assert.Equal(60000, value);
            }
        }

        public class GetMetricsRollingPercentileWindowBuckets
        {
            [Fact]
            public void Returns_MetricsRollingPercentileWindowBuckets_AppSetting_As_Integer_For_GroupA_And_DependencyX()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupA", "DependencyX");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                int value = hystrixConfigurationService.GetMetricsRollingPercentileWindowBuckets();

                Assert.Equal(3000, value);
            }

            [Fact]
            public void Returns_MetricsRollingPercentileWindowBuckets_AppSetting_As_Integer_For_GroupB_And_DependencyY()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupB", "DependencyY");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                int value = hystrixConfigurationService.GetMetricsRollingPercentileWindowBuckets();

                Assert.Equal(1500, value);
            }

            [Fact]
            public void Returns_6_If_AppSetting_Is_Not_A_Valid_Integer()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupC", "DependencyZ");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                int value = hystrixConfigurationService.GetMetricsRollingPercentileWindowBuckets();

                Assert.Equal(6, value);
            }

            [Fact]
            public void Returns_6_If_AppSetting_Does_Not_Exist()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("NonExistingGroup", "NonExistingCommand");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                int value = hystrixConfigurationService.GetMetricsRollingPercentileWindowBuckets();

                Assert.Equal(6, value);
            }
        }

        public class GetMetricsRollingPercentileBucketSize
        {
            [Fact]
            public void Returns_MetricsRollingPercentileBucketSize_AppSetting_As_Integer_For_GroupA_And_DependencyX()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupA", "DependencyX");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                int value = hystrixConfigurationService.GetMetricsRollingPercentileBucketSize();

                Assert.Equal(3000, value);
            }

            [Fact]
            public void Returns_MetricsRollingPercentileBucketSize_AppSetting_As_Integer_For_GroupB_And_DependencyY()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupB", "DependencyY");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                int value = hystrixConfigurationService.GetMetricsRollingPercentileBucketSize();

                Assert.Equal(1500, value);
            }

            [Fact]
            public void Returns_100_If_AppSetting_Is_Not_A_Valid_Integer()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupC", "DependencyZ");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                int value = hystrixConfigurationService.GetMetricsRollingPercentileBucketSize();

                Assert.Equal(100, value);
            }

            [Fact]
            public void Returns_100_If_AppSetting_Does_Not_Exist()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("NonExistingGroup", "NonExistingCommand");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                int value = hystrixConfigurationService.GetMetricsRollingPercentileBucketSize();

                Assert.Equal(100, value);
            }
        }

        public class GetHystrixCommandEnabled
        {
            [Fact]
            public void Returns_HystrixCommandEnabled_AppSetting_As_Boolean_For_GroupA_And_DependencyX()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupA", "DependencyX");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                bool value = hystrixConfigurationService.GetHystrixCommandEnabled();

                Assert.True(value);
            }

            [Fact]
            public void Returns_HystrixCommandEnabled_AppSetting_As_Boolean_For_GroupB_And_DependencyY()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupB", "DependencyY");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                bool value = hystrixConfigurationService.GetHystrixCommandEnabled();

                Assert.False(value);
            }

            [Fact]
            public void Returns_True_If_AppSetting_Is_Not_A_Valid_Integer()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupC", "DependencyZ");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                bool value = hystrixConfigurationService.GetHystrixCommandEnabled();

                Assert.True(value);
            }

            [Fact]
            public void Returns_True_If_AppSetting_Does_Not_Exist()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("NonExistingGroup", "NonExistingCommand");
                var hystrixConfigurationService = new HystrixWebConfigConfigurationService(hystrixCommandIdentifier);

                // act
                bool value = hystrixConfigurationService.GetHystrixCommandEnabled();

                Assert.True(value);
            }
        }       
    }
}