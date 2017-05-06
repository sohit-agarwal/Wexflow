using System;
using Wexflow.Core.Service.Contracts;

namespace Wexflow.Clients.Manager
{
    public class WorkflowDataInfo:IComparable
    {
        public int Id { get; }
        public string Name { get; }
        public LaunchType LaunchType { get; }
        public bool IsEnabled { get; }
        public string Description { get; }

        public WorkflowDataInfo(int id, string name, LaunchType launchType, bool isEnabled, string desc)
        {
            Id = id;
            Name = name;
            LaunchType = launchType;
            IsEnabled = isEnabled;
            Description = desc;
        }

        public int CompareTo(object obj)
        {
            var wf = (WorkflowDataInfo)obj;
            return wf.Id.CompareTo(Id);
        }
    }
}
