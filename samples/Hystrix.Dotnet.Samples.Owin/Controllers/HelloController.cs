using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Hystrix.Dotnet.Samples.Owin.Controllers
{
    public delegate Task<string> GetBreakerCommand (CancellationToken cancellationToken);

    //api/hello
    public class HelloController : ApiController
    {
        private readonly GetBreakerCommand getBreakerCommand;

        public HelloController(GetBreakerCommand getBreakerCommand)
        {
            this.getBreakerCommand = getBreakerCommand;
        }

        public async Task<IHttpActionResult> Get(CancellationToken cancellationToken)
        {
            var result = await getBreakerCommand(cancellationToken);

            return Ok($"Hello! Result: {result}");
        }
    }
}
