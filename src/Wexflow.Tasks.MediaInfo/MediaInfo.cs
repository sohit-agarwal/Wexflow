using System;
using Wexflow.Core;
using System.Xml.Linq;
using System.IO;
using System.Threading;

namespace Wexflow.Tasks.MediaInfo
{
    public class MediaInfo : Task
    {
        public MediaInfo(XElement xe, Workflow wf)
            : base(xe, wf)
        {
        }

        public override TaskStatus Run()
        {
            Info("Generating MediaInfo informations...");

            bool success = true;
            bool atLeastOneSucceed = false;

            var files = SelectFiles();

            if (files.Length > 0)
            {
                var mediaInfoPath = Path.Combine(Workflow.WorkflowTempFolder,
                    string.Format("MediaInfo_{0:yyyy-MM-dd-HH-mm-ss-fff}.xml", DateTime.Now));

                var xdoc = new XDocument(new XElement("Files"));
                foreach (FileInf file in files)
                {
                    try
                    {
                        if (xdoc.Root != null)
                        {
                            XElement xfile = new XElement("File",
                                new XAttribute("path", file.Path),
                                new XAttribute("name", file.FileName));

                            MediaInfoLib.MediaInfo mediaInfo = new MediaInfoLib.MediaInfo();
                            mediaInfo.Open(file.Path);
                            mediaInfo.Option("Complete", "1");
                            string info = mediaInfo.Inform();
                            string[] infos = info.Split(new [] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);

                            XElement xgeneral = null;
                            XElement xaudio = null;
                            XElement xvideo = null;

                            // Build xgeneral
                            foreach (var line in infos)
                            {
                                if (line == "General")
                                {
                                    xgeneral = new XElement("General");
                                    continue;
                                }

                                if (line == "Audio" || line == "Video")
                                {
                                    break;
                                }

                                string[] tag = line.Split(':');
                                xgeneral.Add(new XElement("Tag",
                                    new XAttribute("name", tag[0].Trim()),
                                    new XAttribute("value", tag[1].Trim())));

                               
                            }

                            // Build xvideo
                            var xvideoFound = false;
                            foreach (var line in infos)
                            {
                                if (line == "Video")
                                {
                                    xvideoFound = true;
                                    xvideo = new XElement("Video");
                                    continue;
                                }

                                if (line == "Audio")
                                {
                                    break;
                                }

                                if (xvideoFound)
                                {
                                    string[] tag = line.Split(':');
                                    xvideo.Add(new XElement("Tag",
                                        new XAttribute("name", tag[0].Trim()),
                                        new XAttribute("value", tag[1].Trim())));
                                }

                            }

                            // Build xaudio
                            var xaudioFound = false;
                            foreach (var line in infos)
                            {
                                if (line == "Audio")
                                {
                                    xaudioFound = true;
                                    xaudio = new XElement("Audio");
                                    continue;
                                }

                                if (xaudioFound)
                                {
                                    string[] tag = line.Split(':');
                                    xaudio.Add(new XElement("Tag",
                                        new XAttribute("name", tag[0].Trim()),
                                        new XAttribute("value", tag[1].Trim())));
                                }
                            }

                            if (xgeneral != null)
                            {
                                xfile.Add(xgeneral);
                            }

                            if (xvideo != null)
                            {
                                xfile.Add(xvideo);
                            }

                            if (xaudio != null)
                            {
                                xfile.Add(xaudio);
                            }

                            xdoc.Root.Add(xfile);
                        }
                        InfoFormat("MediaInfo of the file {0} generated.", file.Path);
                        
                        if (!atLeastOneSucceed) atLeastOneSucceed = true;
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        ErrorFormat("An error occured while generating the mediaInfo of the file {0}", e, file.Path);
                        success = false;
                    }
                }
                xdoc.Save(mediaInfoPath);
                Files.Add(new FileInf(mediaInfoPath, Id));
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
