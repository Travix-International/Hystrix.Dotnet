using System.Threading.Tasks;
using Xunit;
using System.Net.Http;
using Owin;
using System.Web.Http;
using System.Text;

namespace Hystrix.Dotnet.AspNetCore.UnitTests
{
    class OwinTestConf
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseHystrixStream();
            HttpConfiguration config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();
            app.UseWebApi(config);
        }
    }

    public class HystrixStreamMiddlewareTests
    {
        [Fact]
        public async Task Is_Able_To_Consume_Stream()
        {
            string result;
            byte[] buffer = new byte[100];

            using (var server = Microsoft.Owin.Testing.TestServer.Create<OwinTestConf>())
            {
                using (var client = new HttpClient(server.Handler))
                {
                    using (var response = await client.GetStreamAsync("http://myApi/hystrix.stream"))
                    {
                        var read = await response.ReadAsync(buffer, 0, buffer.Length);
                        result = Encoding.UTF8.GetString(buffer);
                    }
                }
            }

            Assert.True(result.Contains("ping:"));
        }
    }
}