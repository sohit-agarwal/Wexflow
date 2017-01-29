using Nancy;

namespace Wexflow.Clients.Cmd
{
    public interface IWexflowService
    {
        Response GetWorkflows();

        Response StartWorkflow(string id);

        Response StopWorkflow(string id);

        Response SuspendWorkflow(string id);

        Response ResumeWorkflow(string id);

        Response GetWorkflow(string id);
    }
}
