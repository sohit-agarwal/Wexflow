using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using Wexflow.Core.LiteDB;
using Wexflow.Scripts.Core;

namespace Wexflow.Scripts.LiteDB
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.OSVersion.Platform}.json", optional: true, reloadOnChange: true)
                .Build();

                var workflowsFolder = config["workflowsFolder"];
                Db db = new Db(config["connectionString"]);

                Helper.InsertWorkflowsAndUser(db, workflowsFolder);

                BuildDatabase("Windows", "windows");
                BuildDatabase("Linux", "linux");
                BuildDatabase("Mac OS X", "macos");
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occured: {0}", e);
            }

            Console.Write("Press any key to exit...");
            Console.ReadKey();
        }

        private static void BuildDatabase(string info, string platformFolder)
        {
            Console.WriteLine($"=== Build {info} database ===");
            var dbPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "..",
                "samples", "dotnet-core", platformFolder, "Wexflow", "Database", "Wexflow.db");

            var workflowsFolder = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "..",
                "samples", "MongoDB", "dotnet-core", platformFolder);

            if (!Directory.Exists(workflowsFolder)) throw new DirectoryNotFoundException("Invalid workflows folder: " + workflowsFolder);
            if (File.Exists(dbPath)) File.Delete(dbPath);

            var db = new Db(dbPath);
            Helper.InsertWorkflowsAndUser(db, workflowsFolder);
        }
    }
}
