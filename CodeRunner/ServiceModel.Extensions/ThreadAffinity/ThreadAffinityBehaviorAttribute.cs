using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Description;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace CodeRunner.ServiceModel.ThreadAffinity
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ThreadAffinityBehaviorAttribute :
        Attribute,
        IContractBehavior,
        IServiceBehavior
    {
        Type m_SeriviceType;
        string m_ThreadName;

        public ThreadAffinityBehaviorAttribute(Type serviceType)
            : this(serviceType, null) { }
        public ThreadAffinityBehaviorAttribute(Type serviceType, string threadName)
        {
            m_SeriviceType = serviceType;
            m_ThreadName = threadName ??
                string.Format("Executing endpoints of {0}", m_SeriviceType);
        }

        public string ThreadName
        {
            get { return m_ThreadName; }
            set { m_ThreadName = value; }
        }

        #region IContractBehavior
        void IContractBehavior.AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }
        void IContractBehavior.ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, ClientRuntime clientRuntime) { }
        void IContractBehavior.ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, DispatchRuntime dispatchRuntime)
        {
            ThreadAffinityHelper.ApplyDispatchBehavior(m_SeriviceType, m_ThreadName, dispatchRuntime);
        }
        void IContractBehavior.Validate(ContractDescription contractDescription, ServiceEndpoint endpoint) { }
        #endregion

        #region IServiceBehavior Members
        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters) { }
        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase) { }
        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            // Shut down the worker thread when the host is closed
            serviceHostBase.Closed += delegate
            {
                ThreadAffinityHelper.CloseThread(m_SeriviceType);
            };
        }
        #endregion
    }
}
