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
        ServiceHost<MySingleton> host;

        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext) { }

        // Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup()]
        public static void MyClassCleanup() { }

        // Use TestInitialize to run code before running each test 
        [TestInitialize()]
        public void MyTestInitialize()
        {
            address = "net.pipe://localhost/" + Guid.NewGuid().ToString();
            binding = new NetNamedPipeBinding() { TransactionFlow = true };
            host = new ServiceHost<MySingleton>();
            host.AddServiceEndpoint<ICounter>(binding, address);
            host.Open();
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
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
            Transactional<int> counter = new Transactional<int>();

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
            using (TransactionScope scope = new TransactionScope())
            using (CounterClient proxy = new CounterClient(binding, address))
            {
                Assert.AreEqual<int>(1, proxy.NextValue());
                Assert.AreEqual<int>(2, proxy.NextValue());
                scope.Complete();
            }
            using (TransactionScope scope = new TransactionScope())
            using (CounterClient proxy = new CounterClient(binding, address))
            {
                Assert.AreEqual<int>(3, proxy.NextValue());
                Assert.AreEqual<int>(4, proxy.NextValue());
                // scope.Complete();
            }

            using (TransactionScope scope = new TransactionScope())
            using (CounterClient proxy = new CounterClient(binding, address))
            {
                Assert.AreEqual<int>(3, proxy.NextValue());
                Assert.AreEqual<int>(4, proxy.NextValue());
                scope.Complete();
            }
        }
    }
}
