using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Transactions;
using System.ServiceModel.Channels;

namespace System.ServiceModel.Examples
{
    /// <summary>
    /// Summary description for InstanceManagement
    /// </summary>
    [TestClass]
    public class InstanceManagement
    {
        #region  Service and Client
        [ServiceContract]
        interface IInstanceIdGetter
        {
            [OperationContract]
            Guid GetInstanceId();
        }

        class InstanceIdSetter
        {
            protected Guid instanceId = Guid.NewGuid();
        }

        [ServiceBehavior(ReleaseServiceInstanceOnTransactionComplete = true)]
        class PerCallService : InstanceIdSetter, IInstanceIdGetter
        {

            [OperationBehavior(TransactionScopeRequired = true)]
            public Guid GetInstanceId()
            {
                return base.instanceId;
            }
        }

        [ServiceBehavior(ReleaseServiceInstanceOnTransactionComplete = false)]
        class PerSessionService : InstanceIdSetter, IInstanceIdGetter
        {
            [OperationBehavior(TransactionScopeRequired = true)]
            public Guid GetInstanceId()
            {
                return base.instanceId;
            }
        }

        class ServiceClient : ClientBase<IInstanceIdGetter>, IInstanceIdGetter
        {
            public ServiceClient(Binding binding, string remoteAddress)
                : base(binding, new EndpointAddress(remoteAddress)) { }
            public Guid GetInstanceId()
            { return Channel.GetInstanceId(); }
        }

        #endregion

        NetNamedPipeBinding binding = new NetNamedPipeBinding();

        [TestMethod]
        public void PerCallTransactionService()
        {
            string address = "net.pipe://localhost/" + Guid.NewGuid().ToString();
            using (ServiceHost<PerCallService> host = new ServiceHost<PerCallService>())
            using (ServiceClient proxy = new ServiceClient(binding, address))
            {
                host.AddServiceEndpoint<IInstanceIdGetter>(binding, address);
                host.Open();
                Guid first = proxy.GetInstanceId();
                Guid second = proxy.GetInstanceId();
                Assert.AreNotEqual(second, first);
            }
        }

        [TestMethod]
        public void PerSessionTransactionService()
        {
            string address = "net.pipe://localhost/" + Guid.NewGuid().ToString();
            using (ServiceHost<PerSessionService> host = new ServiceHost<PerSessionService>())
            using (ServiceClient proxy = new ServiceClient(binding, address))
            {
                host.AddServiceEndpoint<IInstanceIdGetter>(binding, address);
                host.Open();
                Guid first = proxy.GetInstanceId();
                Guid second = proxy.GetInstanceId();
                Assert.AreEqual(second, first);
            }
        }
    }
}
