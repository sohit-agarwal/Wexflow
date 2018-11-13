using Nancy;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;

namespace Wexflow.Server
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        private readonly IAppConfiguration appConfig;

        public Bootstrapper()
        {
        }

        public Bootstrapper(IAppConfiguration appConfig)
        {
            this.appConfig = appConfig;
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            container.Register(appConfig);
        }

        protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
        {
            pipelines.AfterRequest += (ctx) =>
            {
                ctx.Response.WithHeader("Access-Control-Allow-Origin", "*")
                    .WithHeader("Access-Control-Allow-Methods", "POST,GET,PUT,DELETE,OPTIONS")
                    .WithHeader("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept, Authorization");
            };
        }

    }
}
