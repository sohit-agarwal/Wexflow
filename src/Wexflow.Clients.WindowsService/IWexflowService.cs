using System.ServiceModel;
using Wexflow.Core.Service.Contracts;

namespace Wexflow.Clients.WindowsService
{
    [ServiceContract(Namespace = "http://wexflow.com/")]
    public interface IWexflowService
    {
        [OperationContract]
        WorkflowInfo[] GetWorkflows();

        [OperationContract]
        void StartWorkflow(string id);

        [OperationContract]
        void StopWorkflow(string id);

        [OperationContract]
        void SuspendWorkflow(string id);

        [OperationContract]
        void ResumeWorkflow(string id);

        [OperationContract]
        WorkflowInfo GetWorkflow(string id);

        [OperationContract]
        TaskInfo[] GetTasks(string id);
    }
}
