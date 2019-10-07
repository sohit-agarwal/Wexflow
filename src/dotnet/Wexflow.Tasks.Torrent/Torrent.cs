using MonoTorrent.Client;
using MonoTorrent.Common;
using System;
using System.Threading;
using System.Xml.Linq;
using Wexflow.Core;

namespace Wexflow.Tasks.Torrent
{
    public class Torrent : Task
    {
        public string SaveFolder { get; set; }

        public Torrent(XElement xe, Workflow wf)
            : base(xe, wf)
        {
            SaveFolder = GetSetting("saveFolder");
        }

        public override TaskStatus Run()
        {
            Info("Downloading torrents...");

            Status status = Status.Success;
            bool succeeded = true;
            bool atLeastOneSuccess = false;
            try
            {
                var torrents = SelectFiles();
                foreach (var torrent in torrents)
                {
                    succeeded &= DownloadTorrent(torrent.Path);
                    if (!atLeastOneSuccess && succeeded) atLeastOneSuccess = true;
                }

                if (!succeeded && atLeastOneSuccess)
                {
                    status = Status.Warning;
                }
                else if (!succeeded)
                {
                    status = Status.Error;
                }

            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while downloading torrents: {0}", e.Message);
                status = Status.Error;
            }

            Info("Task finished.");
            return new TaskStatus(status);
        }

        private bool DownloadTorrent(string path)
        {
            try
            {
                ClientEngine engine = new ClientEngine(new EngineSettings());

                MonoTorrent.Common.Torrent torrent = MonoTorrent.Common.Torrent.Load(@"C:\WexflowTesting\Torrent\sample.torrent");
                TorrentManager torrentManager = new TorrentManager(torrent, @"C:\WexflowTesting\Torrent\", new TorrentSettings());
                engine.Register(torrentManager);
                torrentManager.Start();
                
                // Keep running while the torrent isn't stopped or paused.
                while (torrentManager.State != TorrentState.Stopped && torrentManager.State != TorrentState.Paused)
                {
                    Thread.Sleep(1000);

                    if (torrentManager.Progress == 100.0)
                    {
                        // If we want to stop a torrent, or the engine for whatever reason, we call engine.StopAll()
                        torrentManager.Stop();
                        engine.StopAll();
                        break;
                    }
                }

                InfoFormat("The torrent {0} download succeeded.", path);
                return true;
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                ErrorFormat("An error occured while downloading the torrent {0}: {1}", path, e.Message);
                return false;
            }
        }

    }
}
