using Microsoft.Owin.Hosting;
using Owin;
using System;
using System.Configuration;
using System.Web.Http;

namespace Hystrix.Dotnet.Samples.Owin
{
    class Program
    {
        static void Main(string[] args)
        {
            var url = ConfigurationManager.AppSettings["url"];

            if(string.IsNullOrWhiteSpace(url))
            {
                Console.WriteLine("You must provide \"url\" setting in config");
                return;
            }

            using (WebApp.Start(url, Startup))
            {
                Console.WriteLine("Please call {0}/api/hello", url);
                Console.WriteLine("Stream is here: {0}/hystrix.stream", url);
                Console.Read();
            }
        }

        static void Startup(IAppBuilder app)
        {
            var config = new HttpConfiguration();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            app.UseHystrixStream();

            app.UseWebApi(config);
        }
    }
}
