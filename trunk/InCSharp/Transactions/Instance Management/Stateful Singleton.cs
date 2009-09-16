using System;
using System.ServiceModel.Channels;
using System.Transactions;
using CodeRunner.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ServiceModel.Examples
{
    [TestClass]
    public class StatefulSingletonTests
    {
        #region Additional test attributes
        string address;
        NetNamedPipeBinding binding;
        ServiceHost host;

        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext) { }

        // Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup]
        public static void MyClassCleanup() { }

        // Use TestInitialize to run code before running each test 
        [TestInitialize]
        public void MyTestInitialize()
        {
            address = "net.pipe://localhost/" + Guid.NewGuid();
            binding = new NetNamedPipeBinding { TransactionFlow = true };
            host = new ServiceHost(typeof(MySingleton));
            host.AddServiceEndpoint(typeof(ICounter), binding, address);
            host.Open();
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup]
        public void MyTestCleanup()
        {
            host.Close();
        }
        #endregion

        [ServiceContract]
        interface ICounter
        {
            [OperationContract]
            [TransactionFlow(TransactionFlowOption.Allowed)]
            int NextValue();
        }

        [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
            ReleaseServiceInstanceOnTransactionComplete = false)]
        class MySingleton : ICounter
        {
        	readonly Transactional<int> counter = new Transactional<int>();

            [OperationBehavior(TransactionScopeRequired = true)]
            public int NextValue()
            {
                return ++counter.Value;
            }
        }

        class CounterClient : ClientBase<ICounter>, ICounter
        {
            public CounterClient(Binding binding, string address)
                : base(binding, new EndpointAddress(address)) { }

            public int NextValue()
            { return Channel.NextValue(); }
        }

        [TestMethod]
        public void SingletonTest()
        {
            using (var scope = new TransactionScope())
            using (var proxy = new CounterClient(binding, address))
            {
                Assert.AreEqual(1, proxy.NextValue());
                Assert.AreEqual(2, proxy.NextValue());
                scope.Complete();
            }
            using (var scope = new TransactionScope())
            using (var proxy = new CounterClient(binding, address))
            {
                Assert.AreEqual(3, proxy.NextValue());
                Assert.AreEqual(4, proxy.NextValue());
                // No Complete, transaction will abort and roll back
            }

            using (var scope = new TransactionScope())
            using (var proxy = new CounterClient(binding, address))
            {
                Assert.AreEqual(3, proxy.NextValue());
                Assert.AreEqual(4, proxy.NextValue());
                scope.Complete();
            }
        }
    }
}
