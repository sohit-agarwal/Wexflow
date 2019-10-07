using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.DatabaseRestore
{
    public class DatabaseRestore : Task
    {
        public string ServerConnection { get; set; }
        public string DatabaseName { get; set; }
        public string BackupFilePath { get; set; }

        public DatabaseRestore(XElement xe, Workflow wf) : base(xe, wf)
        {
            ServerConnection = GetSetting("serverConnection");
            DatabaseName = GetSetting("databaseName");
            BackupFilePath = GetSetting("backupFilePath");
        }

        public override TaskStatus Run()
        {
            Info("Restoring database...");
            Status status = Status.Success;
            bool succeeded = false;

            try
            {
                ServerConnection con = new ServerConnection(ServerConnection);
                Server server = new Server(con);
                Restore destination = new Restore();
                destination.Action = RestoreActionType.Database;
                destination.Database = DatabaseName;
                destination.ReplaceDatabase = true;
                destination.Devices.AddDevice(BackupFilePath, DeviceType.File);
                destination.SqlRestore(server);
                succeeded = true;
                InfoFormat("The database {0} was successfully restored.", DatabaseName);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while restoring the database {0} on the server {1}: {2}", DatabaseName, ServerConnection, e.Message);
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
