using System;
using Xunit;

namespace Hystrix.Dotnet.UnitTests
{
    public class CircularArrayTests
    {
        public class Constructor
        {
            [Fact]
            public void Throws_ArgumentOutOfRangeException_When_MaximumSize_Is_Zero()
            {
                // Act
                Assert.Throws<ArgumentOutOfRangeException>(() => new CircularArray<string>(0));
            }

            [Fact]
            public void Sets_MaximumSize_To_Provided_Value_And_Start_With_Length_Zero()
            {
                // Act
                var circularArray = new CircularArray<string>(10);

                Assert.Equal(10, circularArray.MaximumSize);
                Assert.Equal(0, circularArray.Length);
            }
        }

        public class Add
        {
            [Fact]
            public void Increases_Length_By_1_When_Number_Of_Added_Items_Is_Less_Than_MaximumSize()
            {
                var circularArray = new CircularArray<string>(10);

                // Act
                circularArray.Add("1");

                Assert.Equal(1, circularArray.Length);
            }

            [Fact]
            public void Keeps_Length_At_MaximumSize_When_Number_Of_Added_Items_Equals_MaximumSize()
            {
                var circularArray = new CircularArray<string>(10);
                circularArray.Add("1");
                circularArray.Add("2");
                circularArray.Add("3");
                circularArray.Add("4");
                circularArray.Add("5");
                circularArray.Add("6");
                circularArray.Add("7");
                circularArray.Add("8");
                circularArray.Add("9");

                // Act
                circularArray.Add("10");

                Assert.Equal(10, circularArray.Length);
            }

            [Fact]
            public void Keeps_Length_At_MaximumSize_When_Number_Of_Added_Items_Is_Larger_Than_MaximumSize()
            {
                var circularArray = new CircularArray<string>(10);
                circularArray.Add("1");
                circularArray.Add("2");
                circularArray.Add("3");
                circularArray.Add("4");
                circularArray.Add("5");
                circularArray.Add("6");
                circularArray.Add("7");
                circularArray.Add("8");
                circularArray.Add("9");
                circularArray.Add("10");

                // Act
                circularArray.Add("11");

                Assert.Equal(10, circularArray.Length);
            }
        }

        public class GetTail
        {
            [Fact]
            public void Returns_Default_Value_When_No_Item_Has_Been_Added_For_String()
            {
                var circularArray = new CircularArray<string>(10);

                // Act
                string tail = circularArray.GetTail();

                Assert.Null(tail);
            }

            [Fact]
            public void Returns_Default_Value_When_No_Item_Has_Been_Added_For_Integer()
            {
                var circularArray = new CircularArray<int>(10);

                // Act
                int tail = circularArray.GetTail();

                Assert.Equal(0, tail);
            }

            [Fact]
            public void Returns_Default_Value_When_No_Item_Has_Been_Added_For_Boolean()
            {
                var circularArray = new CircularArray<bool>(10);

                // Act
                bool tail = circularArray.GetTail();

                Assert.False(tail);
            }

            [Fact]
            public void Returns_Last_Added_Value()
            {
                var circularArray = new CircularArray<string>(10);
                circularArray.Add("1");

                // Act
                string tail = circularArray.GetTail();

                Assert.Equal("1", tail);
            }

            [Fact]
            public void Returns_Last_Added_Value_When_Number_Of_Added_Items_Equals_MaximumSize()
            {
                var circularArray = new CircularArray<string>(10);
                circularArray.Add("1");
                circularArray.Add("2");
                circularArray.Add("3");
                circularArray.Add("4");
                circularArray.Add("5");
                circularArray.Add("6");
                circularArray.Add("7");
                circularArray.Add("8");
                circularArray.Add("9");
                circularArray.Add("10");

                // Act
                string tail = circularArray.GetTail();

                Assert.Equal("10", tail);
            }

            [Fact]
            public void Returns_Last_Added_Value_When_Number_Of_Added_Items_Is_Larger_Than_MaximumSize()
            {
                var circularArray = new CircularArray<string>(10);
                circularArray.Add("1");
                circularArray.Add("2");
                circularArray.Add("3");
                circularArray.Add("4");
                circularArray.Add("5");
                circularArray.Add("6");
                circularArray.Add("7");
                circularArray.Add("8");
                circularArray.Add("9");
                circularArray.Add("10");
                circularArray.Add("11");

                // Act
                string tail = circularArray.GetTail();

                Assert.Equal("11", tail);
            }
        }

        public class GetArray
        {
            [Fact]
            public void Returns_An_Array_Of_Length_Equal_To_Number_Of_Added_Items_When_Less_Than_Or_Equal_Too_MaximumSize_Items_Are_Added()
            {
                var circularArray = new CircularArray<string>(10);
                circularArray.Add("1");
                circularArray.Add("2");
                circularArray.Add("3");
                circularArray.Add("4");
                circularArray.Add("5");
                circularArray.Add("6");
                circularArray.Add("7");
                circularArray.Add("8");

                // Act
                var array = circularArray.GetArray();

                Assert.Equal(8, array.Length);
            }

            [Fact]
            public void Returns_An_Array_Where_First_Item_Equals_The_First_Item_Added_When_Less_Than_Or_Equal_Too_MaximumSize_Items_Are_Added()
            {
                var circularArray = new CircularArray<string>(10);
                circularArray.Add("1");
                circularArray.Add("2");
                circularArray.Add("3");
                circularArray.Add("4");
                circularArray.Add("5");
                circularArray.Add("6");
                circularArray.Add("7");
                circularArray.Add("8");
                circularArray.Add("9");
                circularArray.Add("10");

                // Act
                var array = circularArray.GetArray();

                Assert.Equal("1", array[0]);
            }

            [Fact]
            public void Returns_An_Array_Where_Last_Item_Equals_The_Last_Item_Added_When_Less_Than_Or_Equal_Too_MaximumSize_Items_Are_Added()
            {
                var circularArray = new CircularArray<string>(10);
                circularArray.Add("1");
                circularArray.Add("2");
                circularArray.Add("3");
                circularArray.Add("4");
                circularArray.Add("5");
                circularArray.Add("6");
                circularArray.Add("7");
                circularArray.Add("8");
                circularArray.Add("9");
                circularArray.Add("10");

                // Act
                var array = circularArray.GetArray();

                Assert.Equal("10", array[9]);
            }

            [Fact]
            public void Returns_An_Array_Of_Length_MaximumSize_When_More_Items_Than_MaximumSize_Are_Added()
            {
                var circularArray = new CircularArray<string>(10);
                circularArray.Add("1");
                circularArray.Add("2");
                circularArray.Add("3");
                circularArray.Add("4");
                circularArray.Add("5");
                circularArray.Add("6");
                circularArray.Add("7");
                circularArray.Add("8");
                circularArray.Add("9");
                circularArray.Add("10");
                circularArray.Add("11");

                // Act
                var array = circularArray.GetArray();

                Assert.Equal(10, array.Length);
            }

            [Fact]
            public void Returns_An_Array_Where_First_Item_Equals_The_Item_Added_MaximumSize_Items_Ago_When_More_Items_Than_MaximumSize_Are_Added()
            {
                var circularArray = new CircularArray<string>(10);
                circularArray.Add("1");
                circularArray.Add("2");
                circularArray.Add("3");
                circularArray.Add("4");
                circularArray.Add("5");
                circularArray.Add("6");
                circularArray.Add("7");
                circularArray.Add("8");
                circularArray.Add("9");
                circularArray.Add("10");
                circularArray.Add("11");

                // Act
                var array = circularArray.GetArray();

                Assert.Equal("2", array[0]);
            }

            [Fact]
            public void Returns_An_Array_Where_Last_Item_Equals_The_Last_Item_Added_When_More_Items_Than_MaximumSize_Are_Added()
            {
                var circularArray = new CircularArray<string>(10);
                circularArray.Add("1");
                circularArray.Add("2");
                circularArray.Add("3");
                circularArray.Add("4");
                circularArray.Add("5");
                circularArray.Add("6");
                circularArray.Add("7");
                circularArray.Add("8");
                circularArray.Add("9");
                circularArray.Add("10");
                circularArray.Add("11");

                // Act
                var array = circularArray.GetArray();

                Assert.Equal("11", array[9]);
            }
        }

        public class Clear
        {
            [Fact]
            public void Removes_All_Buckets()
            {
                var circularArray = new CircularArray<string>(10);
                circularArray.Add("1");
                circularArray.Add("2");
                circularArray.Add("3");
                circularArray.Add("4");
                circularArray.Add("5");
                circularArray.Add("6");
                circularArray.Add("7");
                circularArray.Add("8");
                circularArray.Add("9");
                circularArray.Add("10");
                circularArray.Add("11");

                // Act
                circularArray.Clear();

                Assert.Equal(0, circularArray.Length);
            }
        }
    }
}
