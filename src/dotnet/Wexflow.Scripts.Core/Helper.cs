using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Wexflow.Core.Db;

namespace Wexflow.Scripts.Core
{
    public class Helper
    {
        public static void InsertWorkflowsAndUser(Db db)
        {
            try
            {
                var workflowFiles = Directory.GetFiles(ConfigurationManager.AppSettings["workflowsFolder"]);

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
                        try
                        {
                            db.InsertWorkflow(new Workflow { Xml = xdoc1.ToString() });
                            Console.WriteLine("Workflow " + workflowIdFromFile + " inserted.");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("An error occured: {0}", e);
                        }
                    }
                }
                Console.WriteLine("Workflows created.");
                Console.WriteLine();

                Console.WriteLine("Creating wexflow user...");
                var user = db.GetUser("wexflow");
                if (user == null)
                {
                    db.InsertUser(new User
                    {
                        CreatedOn = DateTime.Now,
                        Username = "wexflow",
                        Password = Db.GetMd5("wexflow2018"),
                        UserProfile = UserProfile.Administrator
                    });
                    user = db.GetUser("wexflow");

                    var workflows = db.GetWorkflows().ToList();
                    foreach (var workflow in workflows)
                    {
                        db.InsertUserWorkflowRelation(new UserWorkflow
                        {
                            UserId = user.GetId(),
                            WorkflowId = workflow.GetDbId()
                        });
                        Console.WriteLine("UserWorkflowRelation ({0}, {1}) created.", user.GetId(), workflow.GetDbId());
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
        }
    }
}
