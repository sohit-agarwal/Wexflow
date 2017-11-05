using System;
using Wexflow.Core;
using System.Xml.Linq;
using Microsoft.Synchronization;
using Microsoft.Synchronization.Files;
using System.Threading;
using System.IO;

namespace Wexflow.Tasks.Sync
{
    public class Sync:Task
    {
        public string SrcFolder { get; private set; }
        public string DestFolder { get; private set; }

        public Sync(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            SrcFolder = GetSetting("srcFolder");
            DestFolder = GetSetting("destFolder");
        }

        public override TaskStatus Run()
        {
            Info("Synchronising folders...");

            bool success = true;

            try
            {
                const string idFileName = "filesync.id";

                var replica1Id = GetReplicaId(Path.Combine(SrcFolder, idFileName));
                var replica2Id = GetReplicaId(Path.Combine(DestFolder, idFileName));

                // Set options for the sync operation
                const FileSyncOptions options = FileSyncOptions.ExplicitDetectChanges |
                                                FileSyncOptions.RecycleDeletedFiles | FileSyncOptions.RecyclePreviousFileOnUpdates | FileSyncOptions.RecycleConflictLoserFiles;

                var filter = new FileSyncScopeFilter();
                filter.FileNameExcludes.Add(idFileName); // Exclude the id file

                // Explicitly detect changes on both replicas upfront, to avoid two change 
                // detection passes for the two-way sync
                DetectChangesOnFileSystemReplica(replica1Id, SrcFolder, filter, options);
                DetectChangesOnFileSystemReplica(replica2Id, DestFolder, filter, options);

                // Sync source to destination
                SyncFileSystemReplicasOneWay(replica1Id, replica2Id, SrcFolder, DestFolder, filter, options);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Error from File Sync Provider: {0}\n", e.Message);
                success = false;
            }

            var status = Status.Success;

            if (!success)
            {
                status = Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status, false);
        }

        public void DetectChangesOnFileSystemReplica(
            Guid replicaId, string replicaRootPath,
            FileSyncScopeFilter filter, FileSyncOptions options)
        {
            FileSyncProvider provider = null;

            try
            {
                provider = new FileSyncProvider(replicaId, replicaRootPath, filter, options);
                provider.DetectChanges();
            }
            finally
            {
                // Release resources
                if (provider != null)
                {
                    provider.Dispose();
                }
            }
        }

        public void SyncFileSystemReplicasOneWay(
                Guid sourceReplicaId, Guid destinationReplicaId,
                string sourceReplicaRootPath, string destinationReplicaRootPath,
                FileSyncScopeFilter filter, FileSyncOptions options)
        {
            FileSyncProvider sourceProvider = null;
            FileSyncProvider destinationProvider = null;

            try
            {
                sourceProvider = new FileSyncProvider(
                    sourceReplicaId, sourceReplicaRootPath, filter, options);
                destinationProvider = new FileSyncProvider(
                    destinationReplicaId, destinationReplicaRootPath, filter, options);

                destinationProvider.AppliedChange += OnAppliedChange;
                destinationProvider.SkippedChange += OnSkippedChange;

                var agent = new SyncOrchestrator
                    {
                        LocalProvider = sourceProvider,
                        RemoteProvider = destinationProvider,
                        Direction = SyncDirectionOrder.Upload
                    };

                InfoFormat("Synchronizing changes to replica: {0}" , destinationProvider.RootDirectoryPath);
                agent.Synchronize();
            }
            finally
            {
                // Release resources
                if (sourceProvider != null) sourceProvider.Dispose();
                if (destinationProvider != null) destinationProvider.Dispose();
            }
        }

        public void OnAppliedChange(object sender, AppliedChangeEventArgs args)
        {
            switch (args.ChangeType)
            {
                case ChangeType.Create:
                    InfoFormat("Applied CREATE for file {0}", args.NewFilePath);
                    break;
                case ChangeType.Delete:
                    InfoFormat("Applied DELETE for file {0}", args.OldFilePath);
                    break;
                case ChangeType.Update:
                    InfoFormat("Applied OVERWRITE for file {0}", args.OldFilePath);
                    break;
                case ChangeType.Rename:
                    InfoFormat("Applied RENAME for file {0} as {1}", args.OldFilePath, args.NewFilePath);
                    break;
            }
        }

        public void OnSkippedChange(object sender, SkippedChangeEventArgs args)
        {
            ErrorFormat("Skipped applying {0} for {1} due to error.", args.ChangeType.ToString().ToUpper(),
                  (!string.IsNullOrEmpty(args.CurrentFilePath) ? args.CurrentFilePath : args.NewFilePath));

            if (args.Exception != null)
                ErrorFormat("Error: {0}", args.Exception.Message);
        }

        public Guid GetReplicaId(string idFilePath)
        {
            Guid replicaId = Guid.Empty;

            if (File.Exists(idFilePath))
            {
                using (StreamReader sr = File.OpenText(idFilePath))
                {
                    var strGuid = sr.ReadLine();
                    if (!string.IsNullOrEmpty(strGuid))
                        replicaId = new Guid(strGuid);
                }
            }

            if (replicaId == Guid.Empty)
            {
                using (FileStream idFile = File.Open(
                            idFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    using (var sw = new StreamWriter(idFile))
                    {
                        replicaId = Guid.NewGuid();
                        sw.WriteLine(replicaId.ToString("D"));
                    }
                }
            }

            return replicaId;
        }
    }
}
