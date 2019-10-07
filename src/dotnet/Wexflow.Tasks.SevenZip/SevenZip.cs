using SevenZip;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.SevenZip
{
    public class SevenZip : Task
    {
        public string ZipFileName { get; private set; }

        public SevenZip(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            ZipFileName = GetSetting("zipFileName");
        }

        public override TaskStatus Run()
        {
            Info("Zipping files...");

            bool success = true;

            var files = SelectFiles();
            if (files.Length > 0)
            {
                var sevenZipPath = Path.Combine(Workflow.WorkflowTempFolder, ZipFileName);

                try
                {
                    SevenZipCompressor sevenZipCompressor = new SevenZipCompressor();
                    sevenZipCompressor.CompressionLevel = CompressionLevel.Ultra;
                    sevenZipCompressor.CompressionMethod = CompressionMethod.Lzma;
                    var filesParam = files.Select(f => f.Path).ToArray();
                    sevenZipCompressor.CompressFiles(sevenZipPath, filesParam);
                    Files.Add(new FileInf(sevenZipPath, Id));
                    InfoFormat("7Z {0} created with success.", sevenZipPath);
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    ErrorFormat("An error occured while creating the 7Z {0}", e, sevenZipPath);
                    success = false;
                }
            }

            var status = Status.Success;

            if (!success)
            {
                status = Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status);
        }
    }
}
