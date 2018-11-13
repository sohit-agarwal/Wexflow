using System;
using System.IO;
using System.Reflection;
using System.Xml;
using log4net;
using log4net.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wexflow.Core;

namespace Wexflow.Server
{
    class Program
    {
        public static WexflowEngine WexflowEngine;

        static void Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            XmlDocument log4netConfig = new XmlDocument();
            log4netConfig.Load(File.OpenRead("log4net.config"));
            var repo = LogManager.CreateRepository(Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));
            XmlConfigurator.Configure(repo, log4netConfig["log4net"]);

            string wexflowSettingsFile = config["WexflowSettingsFile"];
            WexflowEngine = new WexflowEngine(wexflowSettingsFile);
            WexflowEngine.Run();

            var host = new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseKestrel((context, options) =>
                {
                    options.ListenAnyIP(8000);
                })
                .UseUrls("http://localhost:8000/wexflow/")
                .UseStartup<Startup>()
                .Build();

            host.Run();

            Console.WriteLine("Press any key to stop Wexflow workflow server.");
            Console.ReadKey();
        }

        public void ConfigureServices(IServiceCollection services)
        {
        }
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
        }
    }
}
