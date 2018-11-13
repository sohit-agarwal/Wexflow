using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.DatabaseBackup
{
    public class DatabaseBackup : Task
    {
        public string ServerConnection { get; set; }
        public string DatabaseName { get; set; }
        public string BackupFilePath { get; set; }
        public bool Overwrite { get; set; }

        public DatabaseBackup(XElement xe, Workflow wf) : base(xe, wf)
        {
            ServerConnection = GetSetting("serverConnection");
            DatabaseName = GetSetting("databaseName");
            BackupFilePath = GetSetting("backupFilePath");
            Overwrite = bool.Parse(GetSetting("overwrite", "false"));
        }

        public override TaskStatus Run()
        {
            Info("Backuping database...");
            Status status = Status.Success;
            bool succeeded = false;

            try
            {
                ServerConnection con = new ServerConnection(ServerConnection);
                Server server = new Server(con);
                Backup source = new Backup();
                source.Action = BackupActionType.Database;
                source.Database = DatabaseName;
                BackupDeviceItem destination = new BackupDeviceItem(BackupFilePath, DeviceType.File);
                source.Devices.Add(destination);
                if (Overwrite && File.Exists(BackupFilePath)) File.Delete(BackupFilePath);
                source.SqlBackup(server);
                con.Disconnect();
                succeeded = true;
                InfoFormat("The database {0} was successfully backuped in {1}", DatabaseName, BackupFilePath);
                Files.Add(new FileInf(BackupFilePath, Id));
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while backuping the database {0} on the server {1}: {2}", DatabaseName, ServerConnection, e.Message);
                status = Status.Error;
            }

            if (!succeeded)
            {
                status = Status.Error;
            }

            Info("Task finished");
            return new TaskStatus(status);
        }
    }
}
