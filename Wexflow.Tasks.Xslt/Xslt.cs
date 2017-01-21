using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using System.Xml.Linq;
using System.IO;
using System.Xml.Xsl;
using System.Xml.XPath;
using Saxon.Api;
using System.Threading;

namespace Wexflow.Tasks.Xslt
{
    public class Xslt:Task
    {
        public string XsltPath { get; private set; }
        public string Version { get; private set; }
        public bool RemoveWexflowProcessingNodes { get; private set; }

        public Xslt(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            this.XsltPath = this.GetSetting("xsltPath");
            this.Version = this.GetSetting("version");
            this.RemoveWexflowProcessingNodes = bool.Parse(this.GetSetting("removeWexflowProcessingNodes", "true"));
        }

        public override void Run()
        {
            this.Info("Transforming files...");

            foreach (FileInf file in this.SelectFiles())
            {
                string destPath = Path.Combine(this.Workflow.WorkflowTempFolder,
                    string.Format("{0}_{1:yyyy-MM-dd-HH-mm-ss-fff}.xml", Path.GetFileNameWithoutExtension(file.FileName), DateTime.Now));

                try
                {
                    switch (this.Version)
                    {
                        case "1.0":
                            XslCompiledTransform xslt = new XslCompiledTransform();
                            xslt.Load(this.XsltPath);
                            xslt.Transform(file.Path, destPath);
                            this.InfoFormat("File transformed: {0} -> {1}", file.Path, destPath);
                            this.Files.Add(new FileInf(destPath, this.Id));
                            break;
                        case "2.0":
                            // Create a Processor instance.
                            Processor processor = new Processor();

                            // Load the source document.
                            XdmNode input = processor.NewDocumentBuilder().Build(new Uri(file.Path));

                            // Create a transformer for the stylesheet.
                            XsltTransformer transformer = processor.NewXsltCompiler().Compile(new Uri(this.XsltPath)).Load();

                            // Set the root node of the source document to be the initial context node.
                            transformer.InitialContextNode = input;

                            // Create a serializer.
                            Serializer serializer = new Serializer();
                            serializer.SetOutputFile(destPath);

                            // Transform the source XML to System.out.
                            transformer.Run(serializer);
                            this.InfoFormat("File transformed: {0} -> {1}", file.Path, destPath);

                            this.Files.Add(new FileInf(destPath, this.Id));
                            break;
                        default:
                            this.Error("Error in version option. Available options: 1.0 or 2.0");
                            break;
                    }

                    // Set renameTo and tags from /*//<WexflowProcessing>//<File> nodes
                    // Remove /*//<WexflowProcessing> nodes if necessary

                    XDocument xdoc = XDocument.Load(destPath);
                    var xWexflowProcessings = xdoc.Descendants("WexflowProcessing");
                    foreach (var xWexflowProcessing in xWexflowProcessings)
                    {
                        var xFiles = xWexflowProcessing.Descendants("File");
                        foreach (var xFile in xFiles)
                        {
                            try
                            {
                                int taskId = int.Parse(xFile.Attribute("taskId").Value);
                                string fileName = xFile.Attribute("name").Value;
                                XAttribute xRenameTo = xFile.Attribute("renameTo");
                                string renameTo = xRenameTo != null ? xRenameTo.Value : string.Empty;
                                List<Tag> tags = (from xTag in xFile.Attributes()
                                                  where xTag.Name != "taskId" && xTag.Name != "name" && xTag.Name != "renameTo" && xTag.Name != "path" && xTag.Name != "renameToOrName"
                                                  select new Tag(xTag.Name.ToString(), xTag.Value)).ToList();

                                FileInf fileToEdit = (from f in this.Workflow.FilesPerTask[taskId]
                                                      where f.FileName.Equals(fileName)
                                                      select f).FirstOrDefault();

                                if (fileToEdit != null)
                                {
                                    fileToEdit.RenameTo = renameTo;
                                    fileToEdit.Tags.AddRange(tags);
                                    this.InfoFormat("File edited: {0}", fileToEdit.ToString());
                                }
                                else
                                {
                                    this.ErrorFormat("Cannot find the File: {{fileName: {0}, taskId:{1}}}", fileName, taskId);
                                }
                            }
                            catch (ThreadAbortException)
                            {
                                throw;
                            }
                            catch (Exception e)
                            {
                                this.ErrorFormat("An error occured while editing the file: {0}. Error: {1}", xFile.ToString(), e.Message);
                            }
                        }
                    }

                    if (this.RemoveWexflowProcessingNodes)
                    {
                        xWexflowProcessings.Remove();
                        xdoc.Save(destPath);
                    }
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    this.ErrorFormat("An error occured while transforming the file {0}", e, file.Path);
                }
            }

            this.Info("Task finished.");
        }
    }
}
