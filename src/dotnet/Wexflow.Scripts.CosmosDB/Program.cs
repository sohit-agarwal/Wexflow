using System;
using System.Configuration;
using Wexflow.Core.CosmosDB;

namespace Wexflow.Scripts.CosmosDB
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Db db = new Db(ConfigurationManager.AppSettings["connectionString"]);
                Core.Helper.InsertWorkflowsAndUser(db);
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occured: {0}", e);
            }

            Console.Write("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
