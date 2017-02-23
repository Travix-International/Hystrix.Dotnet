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
                // Act
                Assert.Throws<ArgumentNullException>(() => new HystrixCommandIdentifier(null, "CommandX"));
            }

            [Fact]
            public void Throws_ArgumenException_If_GroupKey_Is_Empty_String()
            {
                // Act
                Assert.Throws<ArgumentException>(() => new HystrixCommandIdentifier(string.Empty, "CommandX"));
            }

            [Fact]
            public void Throws_ArgumentNullException_If_CommandKey_Is_Null()
            {
                // Act
                Assert.Throws<ArgumentNullException>(() => new HystrixCommandIdentifier("GroupA", null));
            }

            [Fact]
            public void Throws_ArgumenException_If_CommandKey_Is_Empty_String()
            {
                // Act
                Assert.Throws<ArgumentException>(() => new HystrixCommandIdentifier("GroupA", string.Empty));
            }
        }

        public class GetHashCodeMethod
        {
            [Fact]
            public void Returns_Same_HashCode_Regardless_Of_Casing()
            {
                var firstCommandIdentifier = new HystrixCommandIdentifier("GroupA", "CommandX");
                var secondCommandIdentifier = new HystrixCommandIdentifier("grOUpA", "comMAndX");

                // Act
                var firstHashCode = firstCommandIdentifier.GetHashCode();
                var secondHashCode = secondCommandIdentifier.GetHashCode();

                Assert.Equal(firstHashCode, secondHashCode);
            }

            [Fact]
            public void Returns_Different_HashCode_For_Different_Groups()
            {
                var firstCommandIdentifier = new HystrixCommandIdentifier("GroupA", "CommandX");
                var secondCommandIdentifier = new HystrixCommandIdentifier("GroupB", "CommandX");

                // Act
                var firstHashCode = firstCommandIdentifier.GetHashCode();
                var secondHashCode = secondCommandIdentifier.GetHashCode();

                Assert.NotEqual(firstHashCode, secondHashCode);
            }

            [Fact]
            public void Returns_Different_HashCode_For_Different_Keys()
            {
                var firstCommandIdentifier = new HystrixCommandIdentifier("GroupA", "CommandX");
                var secondCommandIdentifier = new HystrixCommandIdentifier("GroupA", "CommandY");

                // Act
                var firstHashCode = firstCommandIdentifier.GetHashCode();
                var secondHashCode = secondCommandIdentifier.GetHashCode();

                Assert.NotEqual(firstHashCode, secondHashCode);
            }
        }

        public class EqualsMethod
        {
            [Fact]
            public void Returns_True_If_GroupKey_Are_Equal_And_CommandKey_Are_Equal()
            {
                var firstCommandIdentifier = new HystrixCommandIdentifier("GroupA", "CommandX");
                var secondCommandIdentifier = new HystrixCommandIdentifier("GroupA", "CommandX");

                // Act
                var result = firstCommandIdentifier.Equals(secondCommandIdentifier);

                Assert.True(result);
            }

            [Fact]
            public void Returns_True_If_GroupKey_Are_Equal_And_CommandKey_Are_Equal_And_Have_Different_Casing()
            {
                var firstCommandIdentifier = new HystrixCommandIdentifier("GroupA", "CommandX");
                var secondCommandIdentifier = new HystrixCommandIdentifier("grOUpA", "comMAndX");

                // Act
                var result = firstCommandIdentifier.Equals(secondCommandIdentifier);

                Assert.True(result);
            }

            [Fact]
            public void Returns_True_If_GroupKey_Are_Equal_And_CommandKey_Are_Not_Equal()
            {
                var firstCommandIdentifier = new HystrixCommandIdentifier("GroupA", "CommandX");
                var secondCommandIdentifier = new HystrixCommandIdentifier("GroupA", "CommandY");

                // Act
                var result = firstCommandIdentifier.Equals(secondCommandIdentifier);

                Assert.False(result);
            }

            [Fact]
            public void Returns_True_If_GroupKey_Are_Not_Equal_And_CommandKey_Are_Equal()
            {
                var firstCommandIdentifier = new HystrixCommandIdentifier("GroupA", "CommandX");
                var secondCommandIdentifier = new HystrixCommandIdentifier("GroupB", "CommandX");

                // Act
                var result = firstCommandIdentifier.Equals(secondCommandIdentifier);

                Assert.False(result);
            }

            [Fact]
            public void Returns_True_If_GroupKey_Are_Not_Equal_And_CommandKey_Are_Not_Equal()
            {
                var firstCommandIdentifier = new HystrixCommandIdentifier("GroupA", "CommandX");
                var secondCommandIdentifier = new HystrixCommandIdentifier("GroupB", "CommandY");

                // Act
                var result = firstCommandIdentifier.Equals(secondCommandIdentifier);

                Assert.False(result);
            }
        }
    }
}
