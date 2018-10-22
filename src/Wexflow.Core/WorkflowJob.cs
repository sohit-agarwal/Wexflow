using Quartz;

namespace Wexflow.Core
{
    /// <summary>
    /// Quartz Workflow Job.
    /// </summary>
    public class WorkflowJob : IJob
    {
        /// <summary>
        /// Executes workflow the job
        /// </summary>
        /// <param name="context">Job context.</param>
        public void Execute(IJobExecutionContext context)
        {
            Workflow workflow = (Workflow)context.JobDetail.JobDataMap.Get("workflow");
            workflow.Start();
        }
    }
}
