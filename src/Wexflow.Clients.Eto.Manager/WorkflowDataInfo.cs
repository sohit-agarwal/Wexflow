namespace Wexflow.Clients.Eto.Manager
{
    public class WorkflowDataInfo
    {
        public string Id { get; }
        public string Name { get; }
        public string LaunchType { get; }
        public bool IsEnabled { get; }
        public string Description { get; }

        public WorkflowDataInfo(string id, string name, string launchType, bool isEnabled, string desc)
        {
            Id = id;
            Name = name;
            LaunchType = launchType;
            IsEnabled = isEnabled;
            Description = desc;
        }

    }
}
