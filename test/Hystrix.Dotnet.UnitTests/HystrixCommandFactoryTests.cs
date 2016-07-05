using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xunit;

namespace Hystrix.Dotnet.UnitTests
{
    public class HystrixCommandFactoryTests
    {
        public class GetHystrixCommand
        {
            public GetHystrixCommand()
            {
                HystrixCommandFactory.Clear();
            }

            [Fact]
            public void Returns_Instance_Of_Type_IHystrixCommand()
            {
                var factory = new HystrixCommandFactory();

                // act
                var hystrixCommand = factory.GetHystrixCommand(new HystrixCommandIdentifier("groupA", "commandX"));

                Assert.NotNull(hystrixCommand);
                Assert.IsType<HystrixCommand>(hystrixCommand);
            }

            [Fact]
            public void Returns_Same_Instance_For_CommandIdentifier_With_Same_GroupKey_And_CommandKey()
            {
                var factory = new HystrixCommandFactory();
                var firstHystrixCommand = factory.GetHystrixCommand(new HystrixCommandIdentifier("groupA", "commandX"));

                // act
                var secondHystrixCommand = factory.GetHystrixCommand(new HystrixCommandIdentifier("groupA", "commandX"));

                Assert.Same(firstHystrixCommand, secondHystrixCommand);
            }

            [Fact]
            public void Returns_Same_Instance_For_CommandIdentifier_With_Same_GroupKey_And_CommandKey_From_Different_Factories()
            {
                var firstFactory = new HystrixCommandFactory();
                var firstHystrixCommand = firstFactory.GetHystrixCommand(new HystrixCommandIdentifier("groupA", "commandX"));
                var secondFactory = new HystrixCommandFactory();

                // act
                var secondHystrixCommand = secondFactory.GetHystrixCommand(new HystrixCommandIdentifier("groupA", "commandX"));

                Assert.Same(firstHystrixCommand, secondHystrixCommand);
            }

            [Fact]
            public void Returns_Different_Instance_For_Different_CommandIdentifier()
            {
                var factory = new HystrixCommandFactory();
                var firstHystrixCommand = factory.GetHystrixCommand(new HystrixCommandIdentifier("groupA", "commandX"));

                // act
                var secondHystrixCommand = factory.GetHystrixCommand(new HystrixCommandIdentifier("groupA", "commandY"));

                Assert.NotSame(firstHystrixCommand, secondHystrixCommand);
            }

            //[Fact]
            public void LoadTest()
            {
                IHystrixCommandFactory factory = new HystrixCommandFactory();

                Stopwatch stopwatch = Stopwatch.StartNew();
                for (int i = 0; i < 100000; i++)
                {
                    var commandIdentifier = new HystrixCommandIdentifier("group", "key"+ (i%10));
                    var hystrixCommand = factory.GetHystrixCommand(commandIdentifier);
                }

                stopwatch.Stop();
                Assert.Equal(50, stopwatch.Elapsed.Milliseconds); // 540, 114
            }
        }

        public class GetHystrixCommand_With_GroupKey_And_CommandKey
        {
            public GetHystrixCommand_With_GroupKey_And_CommandKey()
            {
                HystrixCommandFactory.Clear();
            }

            [Fact]
            public void Returns_Instance_Of_Type_IHystrixCommand()
            {
                var factory = new HystrixCommandFactory();

                // act
                var hystrixCommand = factory.GetHystrixCommand("groupA", "commandX");

                Assert.NotNull(hystrixCommand);
                Assert.IsType<HystrixCommand>(hystrixCommand);
            }

            [Fact]
            public void Returns_Same_Instance_For_CommandIdentifier_With_Same_GroupKey_And_CommandKey()
            {
                var factory = new HystrixCommandFactory();
                var firstHystrixCommand = factory.GetHystrixCommand("groupA", "commandX");

                // act
                var secondHystrixCommand = factory.GetHystrixCommand("groupA", "commandX");

                Assert.Same(firstHystrixCommand, secondHystrixCommand);
            }

            [Fact]
            public void Returns_Same_Instance_For_CommandIdentifier_With_Same_GroupKey_And_CommandKey_From_Different_Factories()
            {
                var firstFactory = new HystrixCommandFactory();
                var firstHystrixCommand = firstFactory.GetHystrixCommand("groupA", "commandX");
                var secondFactory = new HystrixCommandFactory();

                // act
                var secondHystrixCommand = secondFactory.GetHystrixCommand("groupA", "commandX");

                Assert.Same(firstHystrixCommand, secondHystrixCommand);
            }

            [Fact]
            public void Returns_Different_Instance_For_Different_CommandIdentifier()
            {
                var factory = new HystrixCommandFactory();
                var firstHystrixCommand = factory.GetHystrixCommand("groupA", "commandX");

                // act
                var secondHystrixCommand = factory.GetHystrixCommand("groupA", "commandY");

                Assert.NotSame(firstHystrixCommand, secondHystrixCommand);
            }

            //[Fact]
            public void LoadTest()
            {
                IHystrixCommandFactory factory = new HystrixCommandFactory();

                Stopwatch stopwatch = Stopwatch.StartNew();
                for (int i = 0; i < 100000; i++)
                {
                    var hystrixCommand = factory.GetHystrixCommand("group", "key" + (i % 10));
                }

                stopwatch.Stop();
                Assert.Equal(50, stopwatch.Elapsed.Milliseconds); // 540, 114
            }
        }

        public class GetAllHystrixCommands
        {
            public GetAllHystrixCommands()
            {
                HystrixCommandFactory.Clear();
            }

            [Fact]
            public void Returns_Empty_List_If_No_HystrixCommand_Has_Been_Created()
            {
                var factory = new HystrixCommandFactory();

                // act
                IEnumerable<IHystrixCommand> list = factory.GetAllHystrixCommands();

                Assert.Equal(0, list.Count());
            }

            [Fact]
            public void Returns_List_With_All_Previously_Created_HystrixCommands()
            {
                var factory = new HystrixCommandFactory();
                factory.GetHystrixCommand(new HystrixCommandIdentifier("groupA", "commandX"));
                factory.GetHystrixCommand(new HystrixCommandIdentifier("groupA", "commandY"));

                // act
                IEnumerable<IHystrixCommand> list = factory.GetAllHystrixCommands();

                Assert.Equal(2, list.Count());
            }
        }
    }
}
