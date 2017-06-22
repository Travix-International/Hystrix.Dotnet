using System;
using System.Collections.Generic;
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
                // Act
                Assert.Throws<ArgumentNullException>(() => new HystrixLocalConfigurationService(null, new HystrixLocalOptions()));
            }

            [Fact]
            public void Throws_ArgumentNullException_When_Options_Is_Null()
            {
                // Act
                Assert.Throws<ArgumentNullException>(() => new HystrixLocalConfigurationService(new HystrixCommandIdentifier("a", "b"), null));
            }
        }

        public class GetCommandTimeoutInMilliseconds
        {
            private readonly HystrixLocalOptions options = new HystrixLocalOptions
            {
                CommandGroups = new Dictionary<string, Dictionary<string, HystrixCommandOptions>>
                {
                    ["GroupA"] = new Dictionary<string, HystrixCommandOptions>
                    {
                        ["DependencyX"] = new HystrixCommandOptions
                        {
                            CommandTimeoutInMilliseconds = 1500
                        }
                    }
                }
            };

            [Fact]
            public void Returns_CommandTimeoutInMilliseconds_AppSetting_As_Integer_For_GroupA_And_DependencyX()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupA", "DependencyX");
                var sut = new HystrixLocalConfigurationService(hystrixCommandIdentifier, options);

                // Act
                int result = sut.GetCommandTimeoutInMilliseconds();

                Assert.Equal(1500, result);
            }

            [Fact]
            public void Returns_1000_If_AppSetting_Does_Not_Exist()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("NonExistingGroup", "NonExistingCommand");
                var hystrixConfigurationService = new HystrixLocalConfigurationService(hystrixCommandIdentifier, options);

                // Act
                int value = hystrixConfigurationService.GetCommandTimeoutInMilliseconds();

                Assert.Equal(1000, value);
            }
        }

        public class GetCommandRetryCount
        {
            private readonly HystrixLocalOptions options = new HystrixLocalOptions
            {
                CommandGroups = new Dictionary<string, Dictionary<string, HystrixCommandOptions>>
                {
                    ["GroupA"] = new Dictionary<string, HystrixCommandOptions>
                    {
                        ["DependencyX"] = new HystrixCommandOptions
                        {
                            CommandRetryCount = 2
                        }
                    }
                }
            };

            [Fact]
            public void Returns_CommandRetryCount_AppSetting_As_Integer_For_GroupA_And_DependencyX()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupA", "DependencyX");
                var sut = new HystrixLocalConfigurationService(hystrixCommandIdentifier, options);

                // Act
                int result = sut.GetCommandRetryCount();

                Assert.Equal(2, result);
            }

            [Fact]
            public void Returns_0_If_AppSetting_Does_Not_Exist()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("NonExistingGroup", "NonExistingCommand");
                var hystrixConfigurationService = new HystrixLocalConfigurationService(hystrixCommandIdentifier, options);

                // Act
                int value = hystrixConfigurationService.GetCommandRetryCount();

                Assert.Equal(0, value);
            }
        }

        public class GetCircuitBreakerForcedOpen
        {
            private readonly HystrixLocalOptions options = new HystrixLocalOptions
            {
                CommandGroups = new Dictionary<string, Dictionary<string, HystrixCommandOptions>>
                {
                    ["GroupA"] = new Dictionary<string, HystrixCommandOptions>
                    {
                        ["DependencyX"] = new HystrixCommandOptions
                        {
                            CircuitBreakerForcedOpen = true
                        }
                    }
                }
            };

            [Fact]
            public void Returns_GetCircuitBreakerForcedOpen_AppSetting_As_Boolean_For_GroupA_And_DependencyX()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupA", "DependencyX");
                var hystrixConfigurationService = new HystrixLocalConfigurationService(hystrixCommandIdentifier, options);

                // Act
                bool value = hystrixConfigurationService.GetCircuitBreakerForcedOpen();

                Assert.Equal(true, value);
            }

            [Fact]
            public void Returns_False_If_AppSetting_Does_Not_Exist()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("NonExistingGroup", "NonExistingCommand");
                var hystrixConfigurationService = new HystrixLocalConfigurationService(hystrixCommandIdentifier, options);

                // Act
                bool value = hystrixConfigurationService.GetCircuitBreakerForcedOpen();

                Assert.Equal(false, value);
            }
        }

        public class GetCircuitBreakerForcedClosed
        {
            private readonly HystrixLocalOptions options = new HystrixLocalOptions
            {
                CommandGroups = new Dictionary<string, Dictionary<string, HystrixCommandOptions>>
                {
                    ["GroupA"] = new Dictionary<string, HystrixCommandOptions>
                    {
                        ["DependencyX"] = new HystrixCommandOptions
                        {
                            CircuitBreakerForcedClosed = true
                        }
                    }
                }
            };

            [Fact]
            public void Returns_GetCircuitBreakerForcedClosed_AppSetting_As_Boolean_For_GroupA_And_DependencyX()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupA", "DependencyX");
                var hystrixConfigurationService = new HystrixLocalConfigurationService(hystrixCommandIdentifier, options);

                // Act
                bool value = hystrixConfigurationService.GetCircuitBreakerForcedClosed();

                Assert.Equal(true, value);
            }

            [Fact]
            public void Returns_False_If_AppSetting_Does_Not_Exist()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("NonExistingGroup", "NonExistingCommand");
                var hystrixConfigurationService = new HystrixLocalConfigurationService(hystrixCommandIdentifier, options);

                // Act
                bool value = hystrixConfigurationService.GetCircuitBreakerForcedClosed();

                Assert.Equal(false, value);
            }
        }

        public class GetCircuitBreakerErrorThresholdPercentage
        {
            private readonly HystrixLocalOptions options = new HystrixLocalOptions
            {
                CommandGroups = new Dictionary<string, Dictionary<string, HystrixCommandOptions>>
                {
                    ["GroupA"] = new Dictionary<string, HystrixCommandOptions>
                    {
                        ["DependencyX"] = new HystrixCommandOptions
                        {
                            CircuitBreakerErrorThresholdPercentage = 25
                        }
                    }
                }
            };

            [Fact]
            public void Returns_CircuitBreakerErrorThresholdPercentage_AppSetting_As_Integer_For_GroupA_And_DependencyX()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupA", "DependencyX");
                var hystrixConfigurationService = new HystrixLocalConfigurationService(hystrixCommandIdentifier, options);

                // Act
                int value = hystrixConfigurationService.GetCircuitBreakerErrorThresholdPercentage();

                Assert.Equal(25, value);
            }

            [Fact]
            public void Returns_50_If_AppSetting_Does_Not_Exist()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("NonExistingGroup", "NonExistingCommand");
                var hystrixConfigurationService = new HystrixLocalConfigurationService(hystrixCommandIdentifier, options);

                // Act
                int value = hystrixConfigurationService.GetCircuitBreakerErrorThresholdPercentage();

                Assert.Equal(50, value);
            }
        }

        public class GetCircuitBreakerSleepWindowInMilliseconds
        {
            private readonly HystrixLocalOptions options = new HystrixLocalOptions
            {
                CommandGroups = new Dictionary<string, Dictionary<string, HystrixCommandOptions>>
                {
                    ["GroupA"] = new Dictionary<string, HystrixCommandOptions>
                    {
                        ["DependencyX"] = new HystrixCommandOptions
                        {
                            CircuitBreakerSleepWindowInMilliseconds = 6000
                        }
                    }
                }
            };

            [Fact]
            public void Returns_CircuitBreakerSleepWindowInMilliseconds_AppSetting_As_Integer_For_GroupA_And_DependencyX()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupA", "DependencyX");
                var hystrixConfigurationService = new HystrixLocalConfigurationService(hystrixCommandIdentifier, options);

                // Act
                int value = hystrixConfigurationService.GetCircuitBreakerSleepWindowInMilliseconds();

                Assert.Equal(6000, value);
            }

            [Fact]
            public void Returns_5000_If_AppSetting_Does_Not_Exist()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("NonExistingGroup", "NonExistingCommand");
                var hystrixConfigurationService = new HystrixLocalConfigurationService(hystrixCommandIdentifier, options);

                // Act
                int value = hystrixConfigurationService.GetCircuitBreakerSleepWindowInMilliseconds();

                Assert.Equal(5000, value);
            }
        }

        public class GetCircuitBreakerRequestVolumeThreshold
        {
            private readonly HystrixLocalOptions options = new HystrixLocalOptions
            {
                CommandGroups = new Dictionary<string, Dictionary<string, HystrixCommandOptions>>
                {
                    ["GroupA"] = new Dictionary<string, HystrixCommandOptions>
                    {
                        ["DependencyX"] = new HystrixCommandOptions
                        {
                            CircuitBreakerRequestVolumeThreshold = 50000
                        }
                    }
                }
            };

            [Fact]
            public void Returns_CircuitBreakerRequestVolumeThreshold_AppSetting_As_Integer_For_GroupA_And_DependencyX()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupA", "DependencyX");
                var hystrixConfigurationService = new HystrixLocalConfigurationService(hystrixCommandIdentifier, options);

                // Act
                int value = hystrixConfigurationService.GetCircuitBreakerRequestVolumeThreshold();

                Assert.Equal(50000, value);
            }

            [Fact]
            public void Returns_20_If_AppSetting_Does_Not_Exist()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("NonExistingGroup", "NonExistingCommand");
                var hystrixConfigurationService = new HystrixLocalConfigurationService(hystrixCommandIdentifier, options);

                // Act
                int value = hystrixConfigurationService.GetCircuitBreakerRequestVolumeThreshold();

                Assert.Equal(20, value);
            }
        }

        public class GetMetricsHealthSnapshotIntervalInMilliseconds
        {
            private readonly HystrixLocalOptions options = new HystrixLocalOptions
            {
                CommandGroups = new Dictionary<string, Dictionary<string, HystrixCommandOptions>>
                {
                    ["GroupA"] = new Dictionary<string, HystrixCommandOptions>
                    {
                        ["DependencyX"] = new HystrixCommandOptions
                        {
                            MetricsHealthSnapshotIntervalInMilliseconds = 3000
                        }
                    }
                }
            };

            [Fact]
            public void Returns_MetricsHealthSnapshotIntervalInMilliseconds_AppSetting_As_Integer_For_GroupA_And_DependencyX()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupA", "DependencyX");
                var hystrixConfigurationService = new HystrixLocalConfigurationService(hystrixCommandIdentifier, options);

                // Act
                int value = hystrixConfigurationService.GetMetricsHealthSnapshotIntervalInMilliseconds();

                Assert.Equal(3000, value);
            }

            [Fact]
            public void Returns_500_If_AppSetting_Does_Not_Exist()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("NonExistingGroup", "NonExistingCommand");
                var hystrixConfigurationService = new HystrixLocalConfigurationService(hystrixCommandIdentifier, options);

                // Act
                int value = hystrixConfigurationService.GetMetricsHealthSnapshotIntervalInMilliseconds();

                Assert.Equal(500, value);
            }
        }

        public class GetMetricsRollingStatisticalWindowInMilliseconds
        {
            private readonly HystrixLocalOptions options = new HystrixLocalOptions
            {
                CommandGroups = new Dictionary<string, Dictionary<string, HystrixCommandOptions>>
                {
                    ["GroupA"] = new Dictionary<string, HystrixCommandOptions>
                    {
                        ["DependencyX"] = new HystrixCommandOptions
                        {
                            MetricsRollingStatisticalWindowInMilliseconds = 3000
                        }
                    }
                }
            };

            [Fact]
            public void Returns_MetricsRollingStatisticalWindowInMilliseconds_AppSetting_As_Integer_For_GroupA_And_DependencyX()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupA", "DependencyX");
                var hystrixConfigurationService = new HystrixLocalConfigurationService(hystrixCommandIdentifier, options);

                // Act
                int value = hystrixConfigurationService.GetMetricsRollingStatisticalWindowInMilliseconds();

                Assert.Equal(3000, value);
            }

            [Fact]
            public void Returns_10000_If_AppSetting_Does_Not_Exist()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("NonExistingGroup", "NonExistingCommand");
                var hystrixConfigurationService = new HystrixLocalConfigurationService(hystrixCommandIdentifier, options);

                // Act
                int value = hystrixConfigurationService.GetMetricsRollingStatisticalWindowInMilliseconds();

                Assert.Equal(10000, value);
            }
        }

        public class GetMetricsRollingStatisticalWindowBuckets
        {
            private readonly HystrixLocalOptions options = new HystrixLocalOptions
            {
                CommandGroups = new Dictionary<string, Dictionary<string, HystrixCommandOptions>>
                {
                    ["GroupA"] = new Dictionary<string, HystrixCommandOptions>
                    {
                        ["DependencyX"] = new HystrixCommandOptions
                        {
                            MetricsRollingStatisticalWindowBuckets = 3000
                        }
                    }
                }
            };

            [Fact]
            public void Returns_MetricsRollingStatisticalWindowBuckets_AppSetting_As_Integer_For_GroupA_And_DependencyX()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupA", "DependencyX");
                var hystrixConfigurationService = new HystrixLocalConfigurationService(hystrixCommandIdentifier, options);

                // Act
                int value = hystrixConfigurationService.GetMetricsRollingStatisticalWindowBuckets();

                Assert.Equal(3000, value);
            }

            [Fact]
            public void Returns_10_If_AppSetting_Does_Not_Exist()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("NonExistingGroup", "NonExistingCommand");
                var hystrixConfigurationService = new HystrixLocalConfigurationService(hystrixCommandIdentifier, options);

                // Act
                int value = hystrixConfigurationService.GetMetricsRollingStatisticalWindowBuckets();

                Assert.Equal(10, value);
            }
        }

        public class GetMetricsRollingPercentileEnabled
        {
            private readonly HystrixLocalOptions options = new HystrixLocalOptions
            {
                CommandGroups = new Dictionary<string, Dictionary<string, HystrixCommandOptions>>
                {
                    ["GroupA"] = new Dictionary<string, HystrixCommandOptions>
                    {
                        ["DependencyX"] = new HystrixCommandOptions
                        {
                            MetricsRollingPercentileEnabled = false
                        }
                    }
                }
            };

            [Fact]
            public void Returns_MetricsRollingPercentileEnabled_AppSetting_As_Boolean_For_GroupA_And_DependencyX()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupA", "DependencyX");
                var hystrixConfigurationService = new HystrixLocalConfigurationService(hystrixCommandIdentifier, options);

                // Act
                bool value = hystrixConfigurationService.GetMetricsRollingPercentileEnabled();

                Assert.False(value);
            }

            [Fact]
            public void Returns_True_If_AppSetting_Does_Not_Exist()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("NonExistingGroup", "NonExistingCommand");
                var hystrixConfigurationService = new HystrixLocalConfigurationService(hystrixCommandIdentifier, options);

                // Act
                bool value = hystrixConfigurationService.GetMetricsRollingPercentileEnabled();

                Assert.True(value);
            }
        }

        public class GetMetricsRollingPercentileWindowInMilliseconds
        {
            private readonly HystrixLocalOptions options = new HystrixLocalOptions
            {
                CommandGroups = new Dictionary<string, Dictionary<string, HystrixCommandOptions>>
                {
                    ["GroupA"] = new Dictionary<string, HystrixCommandOptions>
                    {
                        ["DependencyX"] = new HystrixCommandOptions
                        {
                            MetricsRollingPercentileWindowInMilliseconds = 3000
                        }
                    }
                }
            };

            [Fact]
            public void Returns_MetricsRollingPercentileWindowInMilliseconds_AppSetting_As_Integer_For_GroupA_And_DependencyX()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupA", "DependencyX");
                var hystrixConfigurationService = new HystrixLocalConfigurationService(hystrixCommandIdentifier, options);

                // Act
                int value = hystrixConfigurationService.GetMetricsRollingPercentileWindowInMilliseconds();

                Assert.Equal(3000, value);
            }

            [Fact]
            public void Returns_60000_If_AppSetting_Does_Not_Exist()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("NonExistingGroup", "NonExistingCommand");
                var hystrixConfigurationService = new HystrixLocalConfigurationService(hystrixCommandIdentifier, options);

                // Act
                int value = hystrixConfigurationService.GetMetricsRollingPercentileWindowInMilliseconds();

                Assert.Equal(60000, value);
            }
        }

        public class GetMetricsRollingPercentileWindowBuckets
        {
            private readonly HystrixLocalOptions options = new HystrixLocalOptions
            {
                CommandGroups = new Dictionary<string, Dictionary<string, HystrixCommandOptions>>
                {
                    ["GroupA"] = new Dictionary<string, HystrixCommandOptions>
                    {
                        ["DependencyX"] = new HystrixCommandOptions
                        {
                            MetricsRollingPercentileWindowBuckets = 3000
                        }
                    }
                }
            };

            [Fact]
            public void Returns_MetricsRollingPercentileWindowBuckets_AppSetting_As_Integer_For_GroupA_And_DependencyX()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupA", "DependencyX");
                var hystrixConfigurationService = new HystrixLocalConfigurationService(hystrixCommandIdentifier, options);

                // Act
                int value = hystrixConfigurationService.GetMetricsRollingPercentileWindowBuckets();

                Assert.Equal(3000, value);
            }

            [Fact]
            public void Returns_6_If_AppSetting_Does_Not_Exist()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("NonExistingGroup", "NonExistingCommand");
                var hystrixConfigurationService = new HystrixLocalConfigurationService(hystrixCommandIdentifier, options);

                // Act
                int value = hystrixConfigurationService.GetMetricsRollingPercentileWindowBuckets();

                Assert.Equal(6, value);
            }
        }

        public class GetMetricsRollingPercentileBucketSize
        {
            private readonly HystrixLocalOptions options = new HystrixLocalOptions
            {
                CommandGroups = new Dictionary<string, Dictionary<string, HystrixCommandOptions>>
                {
                    ["GroupA"] = new Dictionary<string, HystrixCommandOptions>
                    {
                        ["DependencyX"] = new HystrixCommandOptions
                        {
                            MetricsRollingPercentileBucketSize = 3000
                        }
                    }
                }
            };

            [Fact]
            public void Returns_MetricsRollingPercentileBucketSize_AppSetting_As_Integer_For_GroupA_And_DependencyX()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupA", "DependencyX");
                var hystrixConfigurationService = new HystrixLocalConfigurationService(hystrixCommandIdentifier, options);

                // Act
                int value = hystrixConfigurationService.GetMetricsRollingPercentileBucketSize();

                Assert.Equal(3000, value);
            }

            [Fact]
            public void Returns_100_If_AppSetting_Does_Not_Exist()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("NonExistingGroup", "NonExistingCommand");
                var hystrixConfigurationService = new HystrixLocalConfigurationService(hystrixCommandIdentifier, options);

                // Act
                int value = hystrixConfigurationService.GetMetricsRollingPercentileBucketSize();

                Assert.Equal(100, value);
            }
        }

        public class GetHystrixCommandEnabled
        {
            private readonly HystrixLocalOptions options = new HystrixLocalOptions
            {
                CommandGroups = new Dictionary<string, Dictionary<string, HystrixCommandOptions>>
                {
                    ["GroupA"] = new Dictionary<string, HystrixCommandOptions>
                    {
                        ["DependencyX"] = new HystrixCommandOptions
                        {
                            HystrixCommandEnabled = false
                        }
                    }
                }
            };

            [Fact]
            public void Returns_HystrixCommandEnabled_AppSetting_As_Boolean_For_GroupA_And_DependencyX()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("GroupA", "DependencyX");
                var hystrixConfigurationService = new HystrixLocalConfigurationService(hystrixCommandIdentifier, options);

                // Act
                bool value = hystrixConfigurationService.GetHystrixCommandEnabled();

                Assert.False(value);
            }

            [Fact]
            public void Returns_True_If_AppSetting_Does_Not_Exist()
            {
                var hystrixCommandIdentifier = new HystrixCommandIdentifier("NonExistingGroup", "NonExistingCommand");
                var hystrixConfigurationService = new HystrixLocalConfigurationService(hystrixCommandIdentifier, options);

                // Act
                bool value = hystrixConfigurationService.GetHystrixCommandEnabled();

                Assert.True(value);
            }
        }       
    }
}