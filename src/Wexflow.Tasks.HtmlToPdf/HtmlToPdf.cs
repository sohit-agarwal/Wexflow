using System;
using Wexflow.Core;
using System.Xml.Linq;
using System.IO;
using System.Threading;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using iTextSharp.text.xml.simpleparser;

namespace Wexflow.Tasks.HtmlToPdf
{
    public class HtmlToPdf : Task
    {
        public HtmlToPdf(XElement xe, Workflow wf)
            : base(xe, wf)
        {
        }

        public override TaskStatus Run()
        {
            Info("Generating PDF files from HTML files...");

            bool success = true;
            bool atLeastOneSucceed = false;

            var files = SelectFiles();

            if (files.Length > 0)
            {
                foreach (FileInf file in files)
                {
                    try
                    {
                        string pdfPath = Path.Combine(Workflow.WorkflowTempFolder,
                            string.Format("{0}_{1:yyyy-MM-dd-HH-mm-ss-fff}.pdf", Path.GetFileNameWithoutExtension(file.FileName), DateTime.Now));
                        var doc = new Document();
                        PdfWriter.GetInstance(doc, new FileStream(pdfPath, FileMode.Create));
                        var worker = new HTMLWorker(doc);
                        doc.Open();
                        worker.StartDocument();
                        worker.Parse(new StreamReader(new FileStream(file.Path, FileMode.Open)));
                        worker.EndDocument();
                        worker.Close();
                        // Close the document
                        doc.Close();
                        Files.Add(new FileInf(pdfPath, Id));
                        InfoFormat("PDF {0} generated from the file {1}", pdfPath, file.Path);

                        if (!atLeastOneSucceed) atLeastOneSucceed = true;
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        ErrorFormat("An error occured while generating the md5 of the file {0}", e, file.Path);
                        success = false;
                    }
                }
              
            }

            var status = Status.Success;

            if (!success && atLeastOneSucceed)
            {
                status = Status.Warning;
            }
            else if (!success)
            {
                status = Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status, false);
        }

    }
}
