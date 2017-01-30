using System;

namespace Wexflow.Core.Service.Contracts
{
    public enum LaunchType
    {
        Startup,
        Trigger,
        Periodic
    }

    public class WorkflowInfo:IComparable
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public LaunchType LaunchType { get; private set; }
        public bool IsEnabled { get; private set; }
        public string Description { get; private set; }
        public bool IsRunning { get; set; }
        public bool IsPaused { get; set; }

        public WorkflowInfo(int id, string name, LaunchType launchType, bool isEnabled, string desc, bool isRunning, bool isPaused)
        {
            Id = id;
            Name = name;
            LaunchType = launchType;
            IsEnabled = isEnabled;
            Description = desc;
            IsRunning = isRunning;
            IsPaused = isPaused;
        }

        public int CompareTo(object obj)
        {
            var wfi = (WorkflowInfo)obj;
            return wfi.Id.CompareTo(Id);
        }
    }
}
