using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

        [TestMethod]
        public void PerCallTransactionService()
        {
            NetNamedPipeBinding noFlowBinding = new NetNamedPipeBinding();
            string address = "net.pipe://localhost/" + Guid.NewGuid().ToString();
            using (ServiceHost<MyService> host = new ServiceHost<MyService>())
            using (NoFlowClient proxy = new NoFlowClient(noFlowBinding, address))
            {
                host.AddServiceEndpoint<INoFlow>(noFlowBinding, address);
                host.Open();
                Guid first = proxy.FlowUnspecified().InstanceIdentifier;
                Guid second = proxy.FlowUnspecified().InstanceIdentifier;
                Assert.AreNotEqual(second, first);
            }
        }

        [TestMethod]
        public void PerSessionTransactionService()
        {
            NetNamedPipeBinding noFlowBinding = new NetNamedPipeBinding();
            string address = "net.pipe://localhost/" + Guid.NewGuid().ToString();
            using (ServiceHost<MyService> host = new ServiceHost<MyService>())
            using (NoFlowClient proxy = new NoFlowClient(noFlowBinding, address))
            {
                host.AddServiceEndpoint<INoFlow>(noFlowBinding, address);
                host.Open();
                Guid first = proxy.FlowUnspecified().InstanceIdentifier;
                Guid second = proxy.FlowUnspecified().InstanceIdentifier;
                Assert.AreEqual(second, first);
            }
        }
    }
}
