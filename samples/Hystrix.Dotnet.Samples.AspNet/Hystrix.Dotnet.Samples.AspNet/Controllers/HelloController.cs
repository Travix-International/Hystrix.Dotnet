using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Hystrix.Dotnet.WebConfiguration;

namespace Hystrix.Dotnet.Samples.AspNet.Controllers
{
    public class HelloController : ApiController
    {
        private readonly IHystrixCommand hystrixCommand;

        public HelloController()
        {
            var helper = new AspNetHystrixCommandFactoryHelper();

            var factory = helper.CreateFactory();

            hystrixCommand = factory.GetHystrixCommand("TestGroup", "TestCommand");
        }

        [HttpGet]
        public async Task<IHttpActionResult> Get()
        {
            var result = await hystrixCommand.ExecuteAsync(
                async () =>
                {
                    // Here we could do a potentially failing operation, like calling an external service.
                    var rnd = new Random();
                    await Task.Delay(rnd.Next(500, 1000));

                    if (rnd.Next(4) == 0)
                    {
                        throw new Exception("Test exception. Hystrix will catch this and return the FallbackResult instead.");
                    }

                    return "ExpensiveResult";
                },
                () => Task.FromResult("FallbackResult"),
                new CancellationTokenSource());

            return Ok($"Hello! Result: {result}");
        }
    }
}