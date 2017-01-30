using System.Collections.Generic;
using Wexflow.Core;
using FluentFTP;
using System.Net;

namespace Wexflow.Tasks.Ftp
{
    public class PluginFTPS: PluginBase
    {
		readonly FtpEncryptionMode _encryptionMode;

        public PluginFTPS(Task task, string server, int port, string user, string password, string path, EncryptionMode encryptionMode)
            :base(task, server, port, user, password, path)
        {
            switch (encryptionMode)
            { 
                case EncryptionMode.Explicit:
                    _encryptionMode = FtpEncryptionMode.Explicit;
                    break;
                case EncryptionMode.Implicit:
                    _encryptionMode = FtpEncryptionMode.Implicit;
                    break;
            }
        }

        public override FileInf[] List()
        {
            var files = new List<FileInf>();

            var client = new FtpClient();
            client.Host = Server;
            client.Port = Port;
            client.Credentials = new NetworkCredential(User, Password);
            client.DataConnectionType = FtpDataConnectionType.PASV;
            client.EncryptionMode = _encryptionMode;

            client.Connect();
            client.SetWorkingDirectory(Path);

            var ftpFiles = PluginFTP.ListFiles(client, Task.Id);
            files.AddRange(ftpFiles);

            foreach (var file in files)
                Task.InfoFormat("[PluginFTPS] file {0} found on {1}.", file.Path, Server);

            client.Disconnect();


            return files.ToArray();
        }

        public override void Upload(FileInf file)
        {
            var client = new FtpClient();
            client.Host = Server;
            client.Port = Port;
            client.Credentials = new NetworkCredential(User, Password);
            client.ValidateCertificate += OnValidateCertificate;
            client.DataConnectionType = FtpDataConnectionType.PASV;
            client.EncryptionMode = _encryptionMode;

            client.Connect();
            client.SetWorkingDirectory(Path);

            PluginFTP.UploadFile(client, file);
            Task.InfoFormat("[PluginFTPS] file {0} sent to {1}.", file.Path, Server);

            client.Disconnect();
        }

        static void OnValidateCertificate(FtpClient control, FtpSslValidationEventArgs e)
        {
            e.Accept = true;
        }

        public override void Download(FileInf file)
        {
            var client = new FtpClient();
            client.Host = Server;
            client.Port = Port;
            client.Credentials = new NetworkCredential(User, Password);
            client.EncryptionMode = _encryptionMode;

            client.Connect();
            client.SetWorkingDirectory(Path);

            PluginFTP.DownloadFile(client, file, Task);
            Task.InfoFormat("[PluginFTPS] file {0} downloaded from {1}.", file.Path, Server);

            client.Disconnect();
        }

        public override void Delete(FileInf file)
        {
            var client = new FtpClient();
            client.Host = Server;
            client.Port = Port;
            client.Credentials = new NetworkCredential(User, Password);
            client.EncryptionMode = _encryptionMode;

            client.Connect();
            client.SetWorkingDirectory(Path);

            client.DeleteFile(file.Path);
            Task.InfoFormat("[PluginFTPS] file {0} deleted on {1}.", file.Path, Server);

            client.Disconnect();
        }
    }
}
