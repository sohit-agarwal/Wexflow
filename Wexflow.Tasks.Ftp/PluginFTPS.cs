using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using System.Threading;
using FluentFTP;
using System.Net;

namespace Wexflow.Tasks.Ftp
{
    public class PluginFTPS: PluginBase
    {
        private FtpEncryptionMode _encryptionMode;

        public PluginFTPS(Task task, string server, int port, string user, string password, string path, EncryptionMode encryptionMode)
            :base(task, server, port, user, password, path)
        {
            switch (encryptionMode)
            { 
                case EncryptionMode.Explicit:
                    this._encryptionMode = FtpEncryptionMode.Explicit;
                    break;
                case EncryptionMode.Implicit:
                    this._encryptionMode = FtpEncryptionMode.Implicit;
                    break;
            }
        }

        public override FileInf[] List()
        {
            List<FileInf> files = new List<FileInf>();

            FtpClient client = new FtpClient();
            client.Host = this.Server;
            client.Port = this.Port;
            client.Credentials = new NetworkCredential(this.User, this.Password);
            client.DataConnectionType = FtpDataConnectionType.PASV;
            client.EncryptionMode = this._encryptionMode;

            client.Connect();
            client.SetWorkingDirectory(this.Path);

            FileInf[] ftpFiles = PluginFTP.ListFiles(client, this.Task.Id);
            files.AddRange(ftpFiles);

            foreach (var file in files)
                this.Task.InfoFormat("[PluginFTPS] file {0} found on {1}.", file.Path, this.Server);

            client.Disconnect();


            return files.ToArray();
        }

        public override void Upload(FileInf file)
        {
            FtpClient client = new FtpClient();
            client.Host = this.Server;
            client.Port = this.Port;
            client.Credentials = new NetworkCredential(this.User, this.Password);
            client.ValidateCertificate += OnValidateCertificate;
            client.DataConnectionType = FtpDataConnectionType.PASV;
            client.EncryptionMode = this._encryptionMode;

            client.Connect();
            client.SetWorkingDirectory(this.Path);

            PluginFTP.UploadFile(client, file);
            this.Task.InfoFormat("[PluginFTPS] file {0} sent to {1}.", file.Path, this.Server);

            client.Disconnect();
        }

        static void OnValidateCertificate(FtpClient control, FtpSslValidationEventArgs e)
        {
            e.Accept = true;
        }

        public override void Download(FileInf file)
        {
            FtpClient client = new FtpClient();
            client.Host = this.Server;
            client.Port = this.Port;
            client.Credentials = new NetworkCredential(this.User, this.Password);
            client.EncryptionMode = this._encryptionMode;

            client.Connect();
            client.SetWorkingDirectory(this.Path);

            PluginFTP.DownloadFile(client, file, this.Task);
            this.Task.InfoFormat("[PluginFTPS] file {0} downloaded from {1}.", file.Path, this.Server);

            client.Disconnect();
        }

        public override void Delete(FileInf file)
        {
            FtpClient client = new FtpClient();
            client.Host = this.Server;
            client.Port = this.Port;
            client.Credentials = new NetworkCredential(this.User, this.Password);
            client.EncryptionMode = this._encryptionMode;

            client.Connect();
            client.SetWorkingDirectory(this.Path);

            client.DeleteFile(file.Path);
            this.Task.InfoFormat("[PluginFTPS] file {0} deleted on {1}.", file.Path, this.Server);

            client.Disconnect();
        }
    }
}
