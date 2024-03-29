﻿using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ServiceModel.Examples
{
    [TestClass]
    public class ErrorHandlingTest
    {
        // Contracts
        [ServiceContract]
        interface IMyContract
        {
            [OperationContract]
            void MyMethod();
        }

        // Service
        [ServiceBehavior(IncludeExceptionDetailInFaults = true)] // ExpectedException
        class MyService : IMyContract
        {
            public void MyMethod()
            { 
                throw new FaultException("Untyped Fault."); 
            }
        }

        // Client
        class MyContractClient : ClientBase<IMyContract>, IMyContract
        {
            public MyContractClient() { }
            public MyContractClient(Binding binding, string remoteAddress) :
                base(binding, new EndpointAddress(remoteAddress)) { }

            public void MyMethod()
            { 
                Channel.MyMethod(); 
            }
        }

        #region Host
        // Use ClassInitialize to run code before running the first test in the class
        static NetNamedPipeBinding binding;
        static string address = "net.pipe://localhost/" + Guid.NewGuid().ToString();
        static ServiceHost<MyService> host;
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            binding = new NetNamedPipeBinding();
            host = new ServiceHost<MyService>();
            handler = new BasicErrorHandler();
            host.AddServiceEndpoint<IMyContract>(binding, address);
            host.AddErrorHandler(handler);
            host.Open();
        }

        // Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup()]
        public static void MyClassCleanup()
        {
            host.Close();
        }
        #endregion

        // Basic Error handler
        static BasicErrorHandler handler;
        class BasicErrorHandler : IErrorHandler
        {
            internal bool ProvideFaultCalled { get; set; }
            bool IErrorHandler.HandleError(Exception error)
            { return false; }
            void IErrorHandler.ProvideFault(Exception error, MessageVersion version, ref Message fault)
            { ProvideFaultCalled = true; }
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void BasicErrorHandler_ProvideFault()
        {
            MyContractClient client = new MyContractClient(binding, address);
            try
            {
                client.MyMethod();
            }
            finally
            {
                Assert.IsTrue(handler.ProvideFaultCalled);
                client.Close();
            }
        }
    }
}
