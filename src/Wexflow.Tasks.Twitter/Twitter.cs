using System;
using Wexflow.Core;
using System.Xml.Linq;
using System.Xml.XPath;
using TweetSharp;
using System.Threading;

namespace Wexflow.Tasks.Twitter
{
    public class Twitter:Task
    {
        public string ConsumerKey { get; private set; }
        public string ConsumerSecret { get; private set; }
        public string AccessToken { get; private set; }
        public string AccessTokenSecret { get; private set; }

        public Twitter(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            ConsumerKey = GetSetting("consumerKey");
            ConsumerSecret = GetSetting("consumerSecret");
            AccessToken = GetSetting("accessToken");
            AccessTokenSecret = GetSetting("accessTokenSecret");
        }

        public override TaskStatus Run()
        {
            Info("Sending tweets...");

            bool success = true;
            bool atLeastOneSucceed = false;

            var files = SelectFiles();

            if (files.Length > 0)
            {
                TwitterService service;
                try
                {
                    service = new TwitterService();
                    service.AuthenticateWith(ConsumerKey, ConsumerSecret, AccessToken, AccessTokenSecret);
                    Info("Authentication succeeded.");
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    ErrorFormat("Authentication failed.", e);
                    return new TaskStatus(Status.Error, false);
                }

                foreach (FileInf file in files)
                {
                    try
                    {
                        var xdoc = XDocument.Load(file.Path);
                        foreach (XElement xTweet in xdoc.XPathSelectElements("Tweets/Tweet"))
                        {
                            var status = xTweet.Value;
                            var tweet = service.SendTweet(new SendTweetOptions { Status = status });
                            InfoFormat("Tweet '{0}' sent. id: {1}", status, tweet.Id);
                        }
                        
                        if (!atLeastOneSucceed) atLeastOneSucceed = true;
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        ErrorFormat("An error occured while sending the tweets of the file {0}.", e, file.Path);
                        success = false;
                    }
                }
            }

            var tstatus = Status.Success;

            if (!success && atLeastOneSucceed)
            {
                tstatus = Status.Warning;
            }
            else if (!success)
            {
                tstatus = Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(tstatus, false);
        }
    }
}