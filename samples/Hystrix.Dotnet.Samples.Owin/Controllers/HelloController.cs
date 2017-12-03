using Hystrix.Dotnet.WebConfiguration;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Hystrix.Dotnet.Samples.Owin.Controllers
{
    //api/hello
    public class HelloController : ApiController
    {
        private readonly IHystrixCommand hystrixCommand;
        static readonly Random rnd = new Random();

        public HelloController()
        {
            var helper = new AspNetHystrixCommandFactoryHelper();

            var factory = helper.CreateFactory();

            hystrixCommand = factory.GetHystrixCommand("TestGroup", "TestCommand");
        }

        public async Task<IHttpActionResult> Get(CancellationToken cancellationToken)
        {
            var result = await hystrixCommand.ExecuteAsync(
                    async () =>
                    {
                        // Here we could do a potentially failing operation, like calling an external service.
                        await Task.Delay(rnd.Next(100, 200));

                        if (rnd.Next(2) == 0)
                        {
                            throw new Exception("Test exception. Hystrix will catch this and return the FallbackResult instead.");
                        }

                        return "ExpensiveResult";
                    },
                    () => Task.FromResult("FallbackResult"),
                    CancellationTokenSource.CreateLinkedTokenSource(cancellationToken));

            return Ok($"Hello! Result: {result}");
        }
    }
}
