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
        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region Services
        [ServiceContract]
        interface IMyContract
        {
            [OperationContract]
            Guid GetInstanceId();
        }

        class ServiceInstance
        {
            protected Guid instanceId = Guid.NewGuid();
        }

        [ServiceBehavior(ReleaseServiceInstanceOnTransactionComplete = true)]
        class PerCallService : ServiceInstance, IMyContract
        {

            [OperationBehavior(TransactionScopeRequired = true)]
            public Guid GetInstanceId()
            {
                return base.instanceId;
            }
        }

        [ServiceBehavior(ReleaseServiceInstanceOnTransactionComplete = false)]
        class PerSessionService : ServiceInstance, IMyContract
        {
            [OperationBehavior(TransactionScopeRequired = true)]
            public Guid GetInstanceId()
            {
                return base.instanceId;
            }
        }

        class ServiceClient : ClientBase<IMyContract>, IMyContract
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
                host.AddServiceEndpoint<IMyContract>(binding, address);
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
                host.AddServiceEndpoint<IMyContract>(binding, address);
                host.Open();
                Guid first = proxy.GetInstanceId();
                Guid second = proxy.GetInstanceId();
                Assert.AreEqual(second, first);
            }
        }
    }
}
