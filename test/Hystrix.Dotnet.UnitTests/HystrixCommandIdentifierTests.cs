using System;
using Xunit;

namespace Hystrix.Dotnet.UnitTests
{
    public class HystrixCommandIdentifierTests
    {
        public class Constructor
        {
            [Fact]
            public void Throws_ArgumentNullException_If_GroupKey_Is_Null()
            {
                // act
                Assert.Throws<ArgumentNullException>(() => new HystrixCommandIdentifier(null, "CommandX"));
            }

            [Fact]
            public void Throws_ArgumenException_If_GroupKey_Is_Empty_String()
            {
                // act
                Assert.Throws<ArgumentException>(() => new HystrixCommandIdentifier(string.Empty, "CommandX"));
            }

            [Fact]
            public void Throws_ArgumentNullException_If_CommandKey_Is_Null()
            {
                // act
                Assert.Throws<ArgumentNullException>(() => new HystrixCommandIdentifier("GroupA", null));
            }

            [Fact]
            public void Throws_ArgumenException_If_CommandKey_Is_Empty_String()
            {
                // act
                Assert.Throws<ArgumentException>(() => new HystrixCommandIdentifier("GroupA", string.Empty));
            }
        }

        public class GetHashCodeMethod
        {
            [Fact]
            public void Returns_HashCode_Consisting_Of_GroupKey_And_CommandKey()
            {
                var commandIdentifier = new HystrixCommandIdentifier("GroupA", "CommandX");

                // act
                var hashCode = commandIdentifier.GetHashCode();

                Assert.Equal(579705872, hashCode);
            }

            [Fact]
            public void Returns_Same_HashCode_Regardless_Of_Casing()
            {
                var firstCommandIdentifier = new HystrixCommandIdentifier("GroupA", "CommandX");
                var secondCommandIdentifier = new HystrixCommandIdentifier("grOUpA", "comMAndX");

                // act
                var firstHashCode = firstCommandIdentifier.GetHashCode();
                var secondHashCode = secondCommandIdentifier.GetHashCode();

                Assert.Equal(firstHashCode, secondHashCode);
            }
        }

        public class EqualsMethod
        {
            [Fact]
            public void Returns_True_If_GroupKey_Are_Equal_And_CommandKey_Are_Equal()
            {
                var firstCommandIdentifier = new HystrixCommandIdentifier("GroupA", "CommandX");
                var secondCommandIdentifier = new HystrixCommandIdentifier("GroupA", "CommandX");

                // act
                var result = firstCommandIdentifier.Equals(secondCommandIdentifier);

                Assert.True(result);
            }

            [Fact]
            public void Returns_True_If_GroupKey_Are_Equal_And_CommandKey_Are_Equal_And_Have_Different_Casing()
            {
                var firstCommandIdentifier = new HystrixCommandIdentifier("GroupA", "CommandX");
                var secondCommandIdentifier = new HystrixCommandIdentifier("grOUpA", "comMAndX");

                // act
                var result = firstCommandIdentifier.Equals(secondCommandIdentifier);

                Assert.True(result);
            }

            [Fact]
            public void Returns_True_If_GroupKey_Are_Equal_And_CommandKey_Are_Not_Equal()
            {
                var firstCommandIdentifier = new HystrixCommandIdentifier("GroupA", "CommandX");
                var secondCommandIdentifier = new HystrixCommandIdentifier("GroupA", "CommandY");

                // act
                var result = firstCommandIdentifier.Equals(secondCommandIdentifier);

                Assert.False(result);
            }

            [Fact]
            public void Returns_True_If_GroupKey_Are_Not_Equal_And_CommandKey_Are_Equal()
            {
                var firstCommandIdentifier = new HystrixCommandIdentifier("GroupA", "CommandX");
                var secondCommandIdentifier = new HystrixCommandIdentifier("GroupB", "CommandX");

                // act
                var result = firstCommandIdentifier.Equals(secondCommandIdentifier);

                Assert.False(result);
            }

            [Fact]
            public void Returns_True_If_GroupKey_Are_Not_Equal_And_CommandKey_Are_Not_Equal()
            {
                var firstCommandIdentifier = new HystrixCommandIdentifier("GroupA", "CommandX");
                var secondCommandIdentifier = new HystrixCommandIdentifier("GroupB", "CommandY");

                // act
                var result = firstCommandIdentifier.Equals(secondCommandIdentifier);

                Assert.False(result);
            }
        }
    }
}
