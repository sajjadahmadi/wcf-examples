using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ServiceModel.Test
{
    /// <summary>
    /// Summary description for InProcFactoryTests
    /// </summary>
    [TestClass]
    public class InProcFactoryTests
    {
        public InProcFactoryTests()
        {
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

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
        public void CreateInstanceTest()
        {
            ITestContract proxy1 = InProcFactory.CreateChannel<TestService, ITestContract>();
            Assert.AreEqual<string>("MyResult", proxy1.MyOperation());
            InProcFactory.CloseChannel<ITestContract>(proxy1);
        }

        [TestMethod]
        public void CreateMultipleInstancesTest()
        {
            ITestContract proxy1 = InProcFactory.CreateChannel<TestService, ITestContract>();
            ITestContract proxy2 = InProcFactory.CreateChannel<TestService, ITestContract>();
            Assert.AreNotSame(proxy1, proxy2);
            Assert.AreEqual<string>("MyResult", proxy1.MyOperation());
            Assert.AreEqual<string>("MyResult", proxy2.MyOperation());
        }

        [TestMethod]
        public void NewTest()
        {
            EndpointAddress addr = new EndpointAddress("net.pipe://localhost/Service");

            NetNamedPipeBinding binding = new NetNamedPipeBinding();
            binding.TransactionFlow=true;

            ServiceHost<TestService> host = new ServiceHost<TestService>();
            host.AddServiceEndpoint(typeof(ITestContract), binding, addr.Uri);
            host.Open();

            ITestContract proxy = ChannelFactory<ITestContract>.CreateChannel(binding, addr);
            Assert.AreEqual<string>("MyResult", proxy.MyOperation());

            ((ICommunicationObject)proxy).Close();
            host.Close();
        }

    }
}
