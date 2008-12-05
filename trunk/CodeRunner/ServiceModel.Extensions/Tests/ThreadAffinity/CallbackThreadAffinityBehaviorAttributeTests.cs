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
    #region Service
    [ServiceContract(CallbackContract = typeof(IMyContractCallback))]
    interface IMyContract
    {
        [OperationContract]
        void InvokeCallback();
    }
    interface IMyContractCallback
    {
        [OperationContract]
        void OnCallback();
    }

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
    class MyService : IMyContract
    {
        public void InvokeCallback()
        {
            // Do something
            IMyContractCallback channel = OperationContext.Current.
                GetCallbackChannel<IMyContractCallback>();
            channel.OnCallback();
        }
    } 
    #endregion

    #region Client
    [CallbackThreadAffinityBehavior(typeof(MyCallbackClient), "Callback Thread")]
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
    class MyCallbackClient : IMyContractCallback
    {
        public string m_threadName;
        public void OnCallback()
        {
            // Manually lock local resources in case non-WCF threads try 
            // to access this instances resources.
            lock (this)
            {
                m_threadName = Thread.CurrentThread.Name;
            }
        }
    }

    class MyServiceClient : DuplexClientBase<IMyContract>, IMyContract
    {
        public MyServiceClient(InstanceContext callbackInstance, Binding binding, string remoteAddress)
            : base(callbackInstance, binding, new EndpointAddress(remoteAddress))
        { }

        public void InvokeCallback()
        {
            Channel.InvokeCallback();
        }
    } 
    #endregion

    [TestClass]
    public class CallbackThreadAffinityBehaviorAttributeTests
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
        //[TestInitialize()]
        //public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void CallbackThreadAffinityBehaviorAttributeTest()
        {
            Assert.IsNull(SynchronizationContext.Current);

            string address = "net.pipe://localhost/" + Guid.NewGuid().ToString();
            using (ServiceHost<MyService> host = CreateHost(address))
            {
                MyCallbackClient callback = new MyCallbackClient();
                InstanceContext callbackContext = new InstanceContext(callback);

                MyServiceClient client = new MyServiceClient(callbackContext, new NetNamedPipeBinding(), address);
                client.Open();
                client.InvokeCallback();
                Assert.AreEqual("Callback Thread", callback.m_threadName);
                client.Close();
            }
        }

        ServiceHost<MyService> CreateHost(string address)
        {
            Binding binding = new NetNamedPipeBinding();

            ServiceHost<MyService> host = new ServiceHost<MyService>();
            host.AddServiceEndpoint<IMyContract>(binding, address);
            host.IncludeExceptionDetailInFaults = true;
            host.Open();

            return host;
        }
    }
}
