using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel;
using System.Threading;
using System.ServiceModel.Channels;
using CodeRunner.ServiceModel.ThreadAffinity;

namespace CodeRunner.ServiceModel.Test.ThreadAffinity
{
    interface IMyContractCallback
    {
        [OperationContract]
        void OnCallback();
    }

    [ServiceContract(CallbackContract = typeof(IMyContractCallback))]
    interface IMyContract
    {
        [OperationContract]
        void InvokeCallback();
    }

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
    class MyService : IMyContract
    {
        public void InvokeCallback()
        {
            IMyContractCallback channel = OperationContext.Current.
                GetCallbackChannel<IMyContractCallback>();
            channel.OnCallback();
        }
    }

    [CallbackThreadAffinityBehavior(typeof(MyContractClient), "Callback Thread")]
    class MyContractClient : DuplexClientBase<IMyContract>, IMyContract
    {
        public MyContractClient(InstanceContext callbackInstance, Binding binding, string remoteAddress)
            : base(callbackInstance, binding, new EndpointAddress(remoteAddress))
        { }

        public void InvokeCallback()
        {
            Channel.InvokeCallback();
        }
    }

    [TestClass]
    public class CallbackThreadAffinityBehaviorAttributeTests : IMyContractCallback
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
        [TestInitialize()]
        public void MyTestInitialize() { m_threadName = null; }

        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        internal string m_threadName;

        public void OnCallback()
        {
            m_threadName = Thread.CurrentThread.Name;
        }

        [TestMethod]
        public void CallbackThreadAffinityBehaviorAttributeTest()
        {
            Assert.IsNull(SynchronizationContext.Current);

            Binding binding = new NetNamedPipeBinding();
            string address = "net.pipe://localhost/" + Guid.NewGuid().ToString();

            using (ServiceHost<MyService> host = new ServiceHost<MyService>())
            {
                host.AddServiceEndpoint<IMyContract>(binding, address);
                host.IncludeExceptionDetailInFaults = true;
                host.Open();
                InstanceContext callbackContext = new InstanceContext(this);
                MyContractClient client = new MyContractClient(callbackContext, binding, address);
                client.Open();
                client.InvokeCallback();
                Thread.Sleep(5000);
                Assert.AreEqual("Callback Thread", m_threadName);
                client.Close();
            }
        }
    }
}
