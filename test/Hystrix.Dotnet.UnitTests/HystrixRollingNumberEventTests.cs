using Xunit;

namespace Hystrix.Dotnet.UnitTests
{
    public class HystrixRollingNumberEventTests
    {
        public class IsCounter
        {
            [Fact]
            public void Returns_True_For_Success()
            {
                // Act
                bool isCounter = HystrixRollingNumberEvent.Success.IsCounter();

                Assert.True(isCounter);
            }

            [Fact]
            public void Returns_False_For_CommandMaxActive()
            {
                // Act
                bool isCounter = HystrixRollingNumberEvent.CommandMaxActive.IsCounter();

                Assert.False(isCounter);
            }
        }

        public class IsMaxUpdater
        {
            [Fact]
            public void Returns_False_For_Success()
            {
                // Act
                bool isMaxUpdater = HystrixRollingNumberEvent.Success.IsMaxUpdater();

                Assert.False(isMaxUpdater);
            }

            [Fact]
            public void Returns_True_For_CommandMaxActive()
            {
                // Act
                bool isMaxUpdater = HystrixRollingNumberEvent.CommandMaxActive.IsMaxUpdater();

                Assert.True(isMaxUpdater);
            }
        }
    }
}
