using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wexflow.Core;
using Renci.SshNet;
using System.IO;
using System.Threading;
using Renci.SshNet.Sftp;

namespace Wexflow.Tasks.Ftp
{
    public class PluginSFTP:PluginBase
    {
        public string PrivateKeyPath { get; private set; }
        public string Passphrase { get; private set; }

        public PluginSFTP(Task task, string server, int port, string user, string password, string path, string privateKeyPath, string passphrase)
            :base(task, server, port, user, password, path)
        {
            this.PrivateKeyPath = privateKeyPath;
            this.Passphrase = passphrase;
        }

        private ConnectionInfo GetConnectionInfo()
        {
            // Setup Credentials and Server Information
            ConnectionInfo connInfo = null;

            if (!string.IsNullOrEmpty(this.PrivateKeyPath) && !string.IsNullOrEmpty(this.Passphrase))
            {
                connInfo = new ConnectionInfo(this.Server, this.Port, this.User,
                    new AuthenticationMethod[]{

                        // Pasword based Authentication
                        new PasswordAuthenticationMethod(this.User, this.Password),

                        // Key Based Authentication (using keys in OpenSSH Format)
                        new PrivateKeyAuthenticationMethod(this.User,new PrivateKeyFile[]{ 
                            new PrivateKeyFile(this.PrivateKeyPath, this.Passphrase)
                        })
                });
            }
            else
            {
                connInfo = new ConnectionInfo(this.Server, this.Port, this.User,
                       new AuthenticationMethod[]{

                               // Pasword based Authentication
                               new PasswordAuthenticationMethod(this.User, this.Password)
                           });
            }

            return connInfo;
        }

        public override FileInf[] List()
        {
            List<FileInf> files = new List<FileInf>();

            using (SftpClient client = new SftpClient(this.GetConnectionInfo()))
            {
                client.Connect();
                client.ChangeDirectory(this.Path);

                IEnumerable<SftpFile> sftpFiles = client.ListDirectory(".");
                foreach (SftpFile file in sftpFiles)
                {
                    if (file.IsRegularFile)
                    {
                        files.Add(new FileInf(file.FullName, this.Task.Id));
                        this.Task.InfoFormat("[PluginSFTP] file {0} found on {1}.", file.FullName, this.Server);
                    }
                }

                client.Disconnect();
            }

            return files.ToArray();
        }

        public override void Upload(FileInf file)
        {
            using (SftpClient client = new SftpClient(this.GetConnectionInfo()))
            {
                client.Connect();
                client.ChangeDirectory(this.Path);

                using (FileStream fileStream = File.OpenRead(file.Path))
                {
                    client.UploadFile(fileStream, file.RenameToOrName, true);
                }
                this.Task.InfoFormat("[PluginSFTP] file {0} sent to {1}.", file.Path, this.Server);

                client.Disconnect();
            }
        }

        public override void Download(FileInf file)
        {
            using (SftpClient client = new SftpClient(this.GetConnectionInfo()))
            {
                client.Connect();
                client.ChangeDirectory(this.Path);

                string destFileName = System.IO.Path.Combine(this.Task.Workflow.WorkflowTempFolder, file.FileName);
                using (FileStream ostream = File.Create(destFileName))
                {
                    client.DownloadFile(file.Path, ostream);
                    this.Task.Files.Add(new FileInf(destFileName, this.Task.Id));
                }
                this.Task.InfoFormat("[PluginSFTP] file {0} downloaded from {1}.", file.Path, this.Server);

                client.Disconnect();
            }
        }

        public override void Delete(FileInf file)
        {
            using (SftpClient client = new SftpClient(this.GetConnectionInfo()))
            {
                client.Connect();
                client.ChangeDirectory(this.Path);

                client.DeleteFile(file.Path);
                this.Task.InfoFormat("[PluginSFTP] file {0} deleted from {1}.", file.Path, this.Server);

                client.Disconnect();
            }
        }

    }
}
