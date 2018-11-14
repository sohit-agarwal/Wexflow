namespace Wexflow.Server.Contracts
{
    public class HistoryEntry
    {
        
        public int Id { get; set; }
        
        public int WorkflowId { get; set; }
        
        public string Name { get; set; }
        
        public LaunchType LaunchType { get; set; }
        
        public string Description { get; set; }
        
        public Status Status { get; set; }
        
        public double StatusDate { get; set; }
    }
}
