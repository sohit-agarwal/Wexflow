using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Wexflow.Core;
using Wexflow.Core.Service.Contracts;

namespace Wexflow.Clients.Cmd
{
    [ServiceContract(Namespace = "http://wexflow/")]
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
    }
}
