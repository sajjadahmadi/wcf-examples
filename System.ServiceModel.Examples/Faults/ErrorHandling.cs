using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel.Dispatcher;
using System.Diagnostics;
using System.ServiceModel.Channels;
using System.ServiceModel.Errors;

namespace System.ServiceModel.Examples
{
    class BasicErrorHandler : IErrorHandler
    {
        internal bool ProvideFaultCalled { get; set; }
        bool IErrorHandler.HandleError(Exception error)
        { return false; }
        void IErrorHandler.ProvideFault(Exception error, MessageVersion version, ref Message fault)
        { ProvideFaultCalled = true; }
    }


    [TestClass]
    public class ErrorHandling
    {
        #region Additional test attributes
        // Use ClassInitialize to run code before running the first test in the class
        static NetNamedPipeBinding binding;
        static string address = "net.pipe://localhost/" + Guid.NewGuid().ToString();
        static ServiceHost<MyService> host;
        static BasicErrorHandler handler;

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

        [TestMethod]
        public void ProvideFault()
        {
            MyContractClient client = new MyContractClient(binding, address);
            try
            {
                client.ThrowUntypedFault();
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(FaultException));
            }
            Assert.IsTrue(handler.ProvideFaultCalled);
        }
    }
}
