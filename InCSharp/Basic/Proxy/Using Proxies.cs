using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel;

namespace WcfExamples
{
    [TestClass]
    public class Program
    {
        // Service
        class MyService : IMyContract
        {
            public string MyOperation() {
                return "Return Value";
            }
        }

        #region Host
        static string address = "net.pipe://localhost/" + Guid.NewGuid().ToString();
        static ServiceHost host;

        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext) {
            host = new ServiceHost(typeof(MyService));
            host.AddServiceEndpoint(typeof(IMyContract), new NetNamedPipeBinding(), address);
            host.Open();
        }

        [ClassCleanup()]
        public static void MyClassCleanup() {
            if (host.State == CommunicationState.Opened)
                host.Close();
        }
        #endregion

        [TestMethod]
        public void TestToolGeneratedProxy() {
            ToolGeneratedProxy proxy = new ToolGeneratedProxy(new NetNamedPipeBinding(), new EndpointAddress(address));
            Assert.AreEqual("Return Value", proxy.MyOperation());
            proxy.Close();
        }
    }
}
