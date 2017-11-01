using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wexflow.Tasks.Tests
{
    [TestClass]
    public class StopTest
    {
        [TestInitialize]
        public void TestInitialize()
        {
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void Run()
        {
            int workflowId = 41;
            try
            {
                Helper.StartWorkflowAsync(workflowId);
                Thread.Sleep(500);
                var workflow = Helper.GetWorkflow(workflowId);
                Assert.IsTrue(workflow.IsRunning);
                Helper.StopWorkflow(workflowId);
                Thread.Sleep(500);
                workflow = Helper.GetWorkflow(workflowId);
                Assert.IsFalse(workflow.IsRunning);
            }
            finally
            {
                Helper.StopWorkflow(workflowId);
            }
        }
    }
}
