using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Description;

namespace CodeRunner.ServiceModel.ThreadAffinity
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ThreadAffinityBehaviorAttribute :
        Attribute,
        IContractBehavior
    {
        Type m_SeriviceType;
        string m_ThreadName;

        public ThreadAffinityBehaviorAttribute(Type serviceType)
            : this(serviceType, null)
        { }
        public ThreadAffinityBehaviorAttribute(Type serviceType, string threadName)
        {
            m_SeriviceType = serviceType;
            m_ThreadName = threadName;
        }

        public string ThreadName
        {
            get { return m_ThreadName; }
            set { m_ThreadName = value; }
        }

        #region IContractBehavior
        void IContractBehavior.AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        { }

        void IContractBehavior.ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.ClientRuntime clientRuntime)
        { }

        void IContractBehavior.ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.DispatchRuntime dispatchRuntime)
        {
            string name = m_ThreadName ?? 
                string.Format("Executing endpoints of {0}, {1}", m_SeriviceType, contractDescription.ContractType);
            ThreadAffinityHelper.ApplyDispatchBehavior(m_SeriviceType, name, dispatchRuntime);
        }

        void IContractBehavior.Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
        { }
        #endregion
    }
}
