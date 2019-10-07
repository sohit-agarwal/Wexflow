using System;
using System.Threading;
using System.Xml.Linq;
using System.Xml.XPath;
using Vimeo;
using Wexflow.Core;

namespace Wexflow.Tasks.Vimeo
{
    public class Vimeo : Task
    {
        public string Token { get; }

        public Vimeo(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            Token = GetSetting("token");
        }

        public override TaskStatus Run()
        {
            Info("Uploading videos...");

            bool succeeded = true;
            bool atLeastOneSucceed = false;

            try
            {
                var files = SelectFiles();
                VimeoApi vimeoApi = new VimeoApi(Token);

                foreach (var file in files)
                {
                    try
                    {
                        XDocument xdoc = XDocument.Load(file.Path);

                        foreach (var xvideo in xdoc.XPathSelectElements("/Videos/Video"))
                        {
                            string title = xvideo.Element("Title").Value;
                            string desc = xvideo.Element("Description").Value;
                            string filePath = xvideo.Element("FilePath").Value;

                            try
                            {
                                var videoId = vimeoApi.UploadVideo(filePath, title, desc, (l1, l2) => { });
                                InfoFormat("Video {0} uploaded to Vimeo. VideoId: {1}", filePath, videoId);

                                if (succeeded && !atLeastOneSucceed) atLeastOneSucceed = true;
                            }
                            catch (Exception e)
                            {
                                ErrorFormat("An error occured while uploading the file {0}: {1}", filePath, e.Message);
                                succeeded = false;
                            }
                        }
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        ErrorFormat("An error occured while uploading the file {0}: {1}", file.Path, e.Message);
                        succeeded = false;
                    }
                }

            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while uploading videos: {0}", e.Message);
                return new TaskStatus(Status.Error);
            }

            var status = Status.Success;

            if (!succeeded && atLeastOneSucceed)
            {
                status = Status.Warning;
            }
            else if (!succeeded)
            {
                status = Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status);
        }

    }
}
