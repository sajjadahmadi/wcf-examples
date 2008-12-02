using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace CodeRunner.ServiceModel.ThreadAffinity
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CallbackThreadAffinityBehaviorAttribute : Attribute, IEndpointBehavior
    {
        string m_ThreadName;
        Type m_callbackType;

        public CallbackThreadAffinityBehaviorAttribute(Type callbackType)
            : this(callbackType, null) { }
        public CallbackThreadAffinityBehaviorAttribute(Type callbackType, string threadName)
        {
            m_ThreadName = threadName;
            m_callbackType = callbackType;
            AppDomain.CurrentDomain.ProcessExit += delegate
            {
                ThreadAffinityHelper.CloseThread(m_callbackType);
            };
        }

        public string ThreadName { get; set; }

        #region IEndpointBehavior Members
        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }
        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime) 
        {
            m_ThreadName = m_ThreadName ?? "Executing callbacks of " + m_callbackType;
            ThreadAffinityHelper.ApplyDispatchBehavior(m_callbackType, m_ThreadName, clientRuntime.CallbackDispatchRuntime);
        }
        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) { }
        void IEndpointBehavior.Validate(ServiceEndpoint endpoint) { }
        #endregion
    }
}
