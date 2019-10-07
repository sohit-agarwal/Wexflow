using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace Wexflow.Core.Service.Cross
{
    public class CustomHeaderMessageInspector : IDispatchMessageInspector
    {
        private readonly Dictionary<string, string> _requiredHeaders;

        public CustomHeaderMessageInspector(Dictionary<string, string> headers)
        {
            _requiredHeaders = headers ?? new Dictionary<string, string>();
        }

        public object AfterReceiveRequest(ref Message request, System.ServiceModel.IClientChannel channel, System.ServiceModel.InstanceContext instanceContext)
        {
            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            var httpHeader = reply.Properties["httpResponse"] as HttpResponseMessageProperty;
            foreach (var item in _requiredHeaders)
            {
                if (httpHeader != null) httpHeader.Headers.Add(item.Key, item.Value);
            }
        }
    }
}
