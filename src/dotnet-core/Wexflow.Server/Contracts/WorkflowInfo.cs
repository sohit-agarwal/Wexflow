using System;

namespace Wexflow.Server.Contracts
{
    public enum LaunchType
    {
        Startup,
        Trigger,
        Periodic,
        Cron
    }

    public class WorkflowInfo:IComparable
    {
        public int Id { get;  set; }

        public string Name { get;  set; }

        public LaunchType LaunchType { get;  set; }

        public bool IsEnabled { get;  set; }

        public string Description { get;  set; }

        public bool IsRunning { get; set; }
        
        public bool IsPaused { get; set; }
        
        public string Period { get; set; }
        
        public string CronExpression { get;  set; }
        
        public string Path { get; set; }
        
        public bool IsExecutionGraphEmpty { get; set; }

        public Variable[] LocalVariables { get; set; }

        public WorkflowInfo(int id, string name, LaunchType launchType, bool isEnabled, string desc, bool isRunning, bool isPaused, string period, string cronExpression, string path, bool isExecutionGraphEmpty, Variable[] localVariables)
        {
            Id = id;
            Name = name;
            LaunchType = launchType;
            IsEnabled = isEnabled;
            Description = desc;
            IsRunning = isRunning;
            IsPaused = isPaused;
            Period = period;
            CronExpression = cronExpression;
            Path = path;
            IsExecutionGraphEmpty = isExecutionGraphEmpty;
            LocalVariables = localVariables;
        }

        public int CompareTo(object obj)
        {
            var wfi = (WorkflowInfo)obj;
            return wfi.Id.CompareTo(Id);
        }
    }
}
