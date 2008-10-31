using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel.Dispatcher;
using System.Diagnostics;
using System.ServiceModel.Channels;

namespace System.ServiceModel.Examples
{
    [TestClass]
    public class ErrorHandling
    {
        #region Additional test attributes
        // Use ClassInitialize to run code before running the first test in the class
        static NetNamedPipeBinding binding;
        static string address = "net.pipe://localhost/" + Guid.NewGuid().ToString();
        static ServiceHost<MyService> host;

        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            binding = new NetNamedPipeBinding();
            host = new ServiceHost<MyService>();
            host.AddServiceEndpoint<IMyContract>(binding, address);
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
            BasicErrorHandlerBehaviorAttribute errHandler = new BasicErrorHandlerBehaviorAttribute();
            Assert.IsTrue(errHandler.provideFaultCalled);
        }
    }
}
