namespace Wexflow.Server.Contracts.Workflow
{
    public class Workflow
    {
        //public int Id { get; set; }

        public WorkflowInfo WorkflowInfo { get; set; }

        public TaskInfo[] Tasks { get; set; }
    }
}
