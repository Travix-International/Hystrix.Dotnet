using Hystrix.Dotnet.Samples.Owin.Controllers;
using Hystrix.Dotnet.WebConfiguration;
using Microsoft.Owin.Hosting;
using Owin;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
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

            SetupDI(config);

            app.UseHystrixStream();

            app.UseWebApi(config);
        }

        static void SetupDI(HttpConfiguration conf)
        {
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            var helper = new AspNetHystrixCommandFactoryHelper();

            var factory = helper.CreateFactory();

            container.Register<GetBreakerCommand>(() =>
                (cancel => GetBreakerCommand(factory.GetHystrixCommand("TestGroup", "TestCommand"), cancel)), Lifestyle.Scoped);

            container.Verify();

            conf.DependencyResolver =
                new SimpleInjector.Integration.WebApi.SimpleInjectorWebApiDependencyResolver(container);
        }

        static readonly Random rnd = new Random();

        static Task<string> GetBreakerCommand(IHystrixCommand hystrixCommand, CancellationToken cancellationToken)
        {
            return 
                hystrixCommand.ExecuteAsync(
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
        }
    }
}
