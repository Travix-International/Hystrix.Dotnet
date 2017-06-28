using Hystrix.Dotnet;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace Hystrix.Dotnet.Samples.AspNetCore
{
    [Route("[controller]")]
    [Route("")]
    public class HelloController : Controller
    {
        private readonly IHystrixCommand hystrixCommand;
        
        public HelloController(IHystrixCommandFactory hystrixCommandFactory)
        {
            hystrixCommand = hystrixCommandFactory.GetHystrixCommand("TestGroup", "TestCommand");
        }
        
        [HttpGet]
        public async Task<IActionResult> Get()
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