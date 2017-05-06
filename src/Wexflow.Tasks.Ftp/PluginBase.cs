using Wexflow.Core;

namespace Wexflow.Tasks.Ftp
{
    public enum Protocol
    { 
        Ftp,
        Ftps,
        Sftp
    }

    public abstract class PluginBase
    {
        public string Server { get; }
        public int Port { get; }
        public string User { get; }
        public string Password { get; }
        public string Path { get; }
        public Task Task { get; }

		protected PluginBase(Task task, string server, int port, string user, string password, string path)
        {
            Task = task;
            Server = server;
            Port = port;
            User = user;
            Password = password;
            Path = path;
        }

        public abstract FileInf[] List();
        public abstract void Upload(FileInf file);
        public abstract void Download(FileInf file);
        public abstract void Delete(FileInf file);
    }
}
