namespace Wexflow.Clients.Eto.Manager
{
    public class WorkflowDataInfo
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string LaunchType { get; private set; }
        public bool IsEnabled { get; private set; }
        public string Description { get; private set; }

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