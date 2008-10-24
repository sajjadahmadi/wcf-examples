using System.ServiceModel.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System;

namespace System.ServiceModel.Examples
{


    /// <summary>
    ///This is a test class for MetadataHelperTest and is intended
    ///to contain all MetadataHelperTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MetadataHelperTests
    {


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

        ServiceHost host;
        ServiceEndpoint serviceEndpoint;

        //Use TestInitialize to run code before running each test
        [TestInitialize()]
        public void MyTestInitialize()
        {
            Uri baseAddress = new Uri("net.tcp://localhost:8000");
            host = new ServiceHost(typeof(TestService), baseAddress);
            serviceEndpoint = host.AddServiceEndpoint(typeof(ITestContract), new NetTcpBinding(), "Service");

            BindingElement bindingElement = new TcpTransportBindingElement();
            CustomBinding binding = new CustomBinding(bindingElement);
            host.Description.Behaviors.Add(new ServiceMetadataBehavior());
            host.AddServiceEndpoint(typeof(IMetadataExchange), binding, "");

            host.Open();
        }
        //
        //Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
            host.Close();
        }
        
        #endregion

        /// <summary>
        ///A test for SupportsContract
        ///</summary>
        [TestMethod()]
        public void SupportsContractTest1()
        {
            string mexAddress = "net.tcp://localhost:8000";
            string contractNamespace = "http://tempuri.org/";
            string contractName = "ITestContract";
            bool actual;
            actual = MetadataHelper.SupportsContract(mexAddress, contractNamespace, contractName);
            Assert.AreEqual(true, actual);
        }

        /// <summary>
        ///A test for SupportsContract
        ///</summary>
        [TestMethod()]
        public void SupportsContractTest()
        {
            string mexAddress = "net.tcp://localhost:8000";
            Type contractType = typeof(ITestContract);
            bool actual;
            actual = MetadataHelper.SupportsContract(mexAddress, contractType);
            Assert.AreEqual(true, actual);
        }

        /// <summary>
        ///A test for GetEndpoints
        ///</summary>
        [TestMethod()]
        public void GetEndpointsTest1()
        {
            string mexAddress = "net.tcp://localhost:8000";
            ServiceEndpointCollection actual;
            actual = MetadataHelper.GetEndpoints(mexAddress);
            Assert.IsTrue(actual.Count == 1);
            ServiceEndpoint ep = actual[0];
            Assert.AreEqual(typeof(NetTcpBinding), ep.Binding.GetType());
        }

        /// <summary>
        ///A test for GetEndpoints
        ///</summary>
        [TestMethod()]
        [DeploymentItem("System.ServiceModel.Extensions.dll")]
        public void GetEndpointsTest()
        {
            string mexAddress = "net.tcp://localhost:8000";
            TcpTransportBindingElement bindingElement =
                new TcpTransportBindingElement();
            ServiceEndpointCollection actual;
            actual = MetadataHelper_Accessor.GetEndpoints(mexAddress, bindingElement);
            Assert.IsTrue(actual.Count == 1);
            ServiceEndpoint ep = actual[0];
            Assert.AreEqual(typeof(NetTcpBinding), ep.Binding.GetType());
        }
    }
}
