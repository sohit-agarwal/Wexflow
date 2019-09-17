using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Wexflow.Core.MongoDB;

namespace Wexflow.Scripts.MongoDB
{
    class Program
    {
        static void Main(string[] args)
        {

            try
            {
                IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

                var workflowFiles = Directory.GetFiles(config["workflowsFolder"]);
                Db db = new Db(config["connectionString"]);

                db.Init();

                Console.WriteLine("Creating workflows...");
                foreach (var workflowFile in workflowFiles)
                {
                    XNamespace xn = "urn:wexflow-schema";
                    var xdoc1 = XDocument.Load(workflowFile);
                    var workflowIdFromFile = int.Parse(xdoc1.Element(xn + "Workflow").Attribute("id").Value);

                    var found = false;
                    var workflows = db.GetWorkflows().ToList();
                    foreach (var workflow in workflows)
                    {
                        var xdoc2 = XDocument.Parse(workflow.Xml);
                        var workflowId = int.Parse(xdoc2.Element(xn + "Workflow").Attribute("id").Value);
                        if (workflowIdFromFile == workflowId)
                        {
                            found = true;
                            Console.WriteLine("Workflow " + workflowIdFromFile + " already in database.");
                            break;
                        }
                    }

                    if (!found)
                    {
                        db.InsertWorkflow(new Core.Db.Workflow { Xml = xdoc1.ToString() });
                        Console.WriteLine("Workflow " + workflowIdFromFile + " inserted.");
                    }
                }
                Console.WriteLine("Workflows created.");
                Console.WriteLine();

                Console.WriteLine("Creating wexflow user...");
                var user = db.GetUser("wexflow");
                if (user == null)
                {
                    db.InsertUser(new Core.Db.User
                    {
                        CreatedOn = DateTime.Now,
                        Username = "wexflow",
                        Password = Core.Db.Db.GetMd5("wexflow2018"),
                        UserProfile = Core.Db.UserProfile.Administrator
                    });
                    user = db.GetUser("wexflow");

                    var workflows = db.GetWorkflows().ToList();
                    foreach (var workflow in workflows)
                    {
                        db.InsertUserWorkflowRelation(new Core.Db.UserWorkflow
                        {
                            UserId = user.GetId(),
                            WorkflowId = workflow.GetDbId()
                        });
                    }
                    Console.WriteLine("wexflow user created with success.");
                }
                else
                {
                    Console.WriteLine("The user wexflow already exists.");
                }
                Console.WriteLine();
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
