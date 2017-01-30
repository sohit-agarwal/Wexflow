using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace Wexflow.Core.Service.Cross
{
    public class CustomHeaderMessageInspector : IDispatchMessageInspector
    {
		readonly Dictionary<string, string> requiredHeaders;

        public CustomHeaderMessageInspector(Dictionary<string, string> headers)
        {
            requiredHeaders = headers ?? new Dictionary<string, string>();
        }

        public object AfterReceiveRequest(ref Message request, System.ServiceModel.IClientChannel channel, System.ServiceModel.InstanceContext instanceContext)
        {
            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            var httpHeader = reply.Properties["httpResponse"] as HttpResponseMessageProperty;
            foreach (var item in requiredHeaders)
            {
                httpHeader.Headers.Add(item.Key, item.Value);
            }
        }
    }
}
