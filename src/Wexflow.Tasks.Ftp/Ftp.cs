using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using System.Xml.Linq;
using System.Threading;

namespace Wexflow.Tasks.Ftp
{
    public enum FtpCommad
    { 
        List,
        Upload,
        Download,
        Delete,
    }

    public enum EncryptionMode
    { 
        Explicit,
        Implicit
    }

    public class Ftp: Task
    {
        private PluginBase _plugin;
        private FtpCommad _cmd;
        private int _retryCount;
        private int _retryTimeout;

        public Ftp(XElement xe, Workflow wf): base(xe, wf)
        {
            string server = this.GetSetting("server");
            int port = int.Parse(this.GetSetting("port"));
            string user = this.GetSetting("user");
            string password = this.GetSetting("password");
            string path = this.GetSetting("path");
            Protocol protocol = (Protocol)Enum.Parse(typeof(Protocol), this.GetSetting("protocol"), true);
            switch (protocol)
            { 
                case Protocol.FTP:
                    _plugin = new PluginFTP(this, server, port, user, password, path);
                    break;
                case Protocol.FTPS:
                    EncryptionMode encryptionMode = (EncryptionMode)Enum.Parse(typeof(EncryptionMode), this.GetSetting("encryption"), true);
                    _plugin = new PluginFTPS(this, server, port, user, password, path, encryptionMode);
                    break;
                case Protocol.SFTP:
                    string privateKeyPath = this.GetSetting("privateKeyPath", string.Empty);
                    string passphrase = this.GetSetting("passphrase", string.Empty);
                    _plugin = new PluginSFTP(this, server, port, user, password, path, privateKeyPath, passphrase);
                    break;
            }
            this._cmd = (FtpCommad)Enum.Parse(typeof(FtpCommad), this.GetSetting("command"), true);
            this._retryCount = int.Parse(this.GetSetting("retryCount", "3"));
            this._retryTimeout = int.Parse(this.GetSetting("retryTimeout", "1500"));
        }

        public override TaskStatus Run()
        {
            this.Info("Processing files...");
            
            bool success = true;
            bool atLeastOneSucceed = false;

            if (this._cmd == FtpCommad.List)
            {
                int r = 0;
                while (r <= this._retryCount)
                {
                    try
                    {
                        FileInf[] files = this._plugin.List();
                        this.Files.AddRange(files);
                        success &= true;
                        if (!atLeastOneSucceed) atLeastOneSucceed = true;
                        break;
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        r++;

                        if (r > this._retryCount)
                        {
                            this.ErrorFormat("An error occured while listing files. Error: {0}", e.Message);
                            success &= false;
                        }
                        else
                        {
                            this.InfoFormat("An error occured while listing files. Error: {0}. The task will tray again.", e.Message);
                            Thread.Sleep(this._retryTimeout);
                        }
                    }
                }
            }
            else
            { 
                FileInf[] files = this.SelectFiles();
                for (int i = files.Length - 1; i > -1; i--)
                {
                    FileInf file = files[i];

                    int r = 0;
                    while (r <= this._retryCount)
                    {
                        try
                        {

                            switch (this._cmd)
                            {
                                case FtpCommad.Upload:
                                    this._plugin.Upload(file);
                                    break;
                                case FtpCommad.Download:
                                    this._plugin.Download(file);
                                    break;
                                case FtpCommad.Delete:
                                    this._plugin.Delete(file);
                                    this.Workflow.FilesPerTask[file.TaskId].Remove(file);
                                    break;
                            }

                            success &= true;
                            if (!atLeastOneSucceed) atLeastOneSucceed = true;
                            break;
                        }
                        catch (ThreadAbortException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            r++;

                            if (r > this._retryCount)
                            {
                                this.ErrorFormat("An error occured while processing the file {0}. Error: {1}", file.Path, e.Message);
                                success &= false;
                            }
                            else
                            {
                                this.InfoFormat("An error occured while processing the file {0}. Error: {1}. The task will tray again.", file.Path, e.Message);
                                Thread.Sleep(this._retryTimeout);
                            }
                        }

                    }
                }
            }

            Status status = Status.Success;

            if (!success && atLeastOneSucceed)
            {
                status = Status.Warning;
            }
            else if(!success)
            {
                status = Status.Error;
            }

            this.Info("Task finished.");
            return new TaskStatus(status, false);
        }
    }
}
