using Xunit;

namespace Hystrix.Dotnet.UnitTests
{
    public class LongMaxUpdaterTests
    {
        private LongMaxUpdater num = new LongMaxUpdater();

        [Fact]
        public void LongMaxUpdater_DefaultsToLongMinValue()
        {
            Assert.Equal(long.MinValue, num.Max());
        }

        [Fact]
        public void LongMaxUpdater_CanBeCreatedWithValue()
        {
            Assert.Equal(5L, new LongMaxUpdater(5L).Max());
        }

        [Fact]
        public void LongMaxUpdater_CanGetAndReset()
        {
            this.num.Update(32);
            long val = this.num.MaxThenReset();

            Assert.Equal(32, val);
            Assert.Equal(long.MinValue, num.Max());
        }

        [Fact]
        public void LongMaxUpdater_CanBeUpdated_To_Max()
        {
            this.num.Update(3);
            Assert.Equal(3L, num.Max());
        }

        [Fact]
        public void LongMaxUpdater_CanBeIncrementedMultipleTimes()
        {
            this.num.Update(3);
            this.num.Update(5);
            this.num.Update(4);

            Assert.Equal(5L, num.Max());
        }
    }
}
