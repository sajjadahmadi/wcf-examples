using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ServiceModel.Test
{
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
        public void MyTestInitialize()
        {
            contractNamespace = "http://tempuri.org/";
            contractName = "ITestContract";

            ServiceHost<TestService> host = new ServiceHost<TestService>();

            Binding binding = new WSHttpBinding();
            string address = "http://localhost:8888";
            host.AddServiceEndpoint<ITestContract>(binding, address);

            Binding mexBinding = MetadataExchangeBindings.CreateMexHttpBinding();
            string mexAddress = "http://localhost:8888/mex";
            host.AddServiceEndpoint<IMetadataExchange>(mexBinding, mexAddress);
            host.Description.Behaviors.Add(new ServiceMetadataBehavior());

            host.IncludeExceptionDetailInFaults = true;
            host.Open();

            TestContext.Properties.Add("Host", host);
            TestContext.Properties.Add("Binding", binding);
            TestContext.Properties.Add("MEX Address", mexAddress);
            TestContext.Properties.Add("Service Address", address);
        }
        //
        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
            ServiceHost<TestService> host;
            using (host = TestContext.Properties["Host"] as ServiceHost<TestService>)
            {
                if (host.State == CommunicationState.Opened) host.Close();
            }
        }

        #endregion

        string contractNamespace;
        string contractName;


        #region Service
        [ServiceContract]
        interface ITestContract
        {
            [OperationContract]
            string MyOperation();
        }

        class TestService : ITestContract
        {
            public string MyOperation()
            {
                return "MyResult";
            }
        }
        #endregion

        /// <summary>
        ///A test for SupportsContract
        ///</summary>
        [TestMethod()]
        public void SupportsContractTest1()
        {
            string address = TestContext.Properties["Service Address"].ToString();
            ITestContract proxy = ChannelFactory<ITestContract>.CreateChannel(new WSHttpBinding(), new EndpointAddress(address));
            proxy.MyOperation();
            bool actual;
            string mexAddress = TestContext.Properties["MEX Address"].ToString();
            actual = MetadataHelper.SupportsContract(mexAddress, contractNamespace, contractName);
            Assert.AreEqual(true, actual);
        }

        /// <summary>
        ///A test for SupportsContract
        ///</summary>
        [TestMethod()]
        public void SupportsContractTest()
        {
            Type contractType = typeof(ITestContract);
            bool actual;
            string mexAddress = TestContext.Properties["MEX Address"].ToString();
            actual = MetadataHelper.SupportsContract(mexAddress, contractType);
            Assert.AreEqual(true, actual);
        }

        /// <summary>
        ///A test for GetEndpoints
        ///</summary>
        [TestMethod()]
        public void GetEndpointsTest1()
        {
            ServiceEndpointCollection actual;
            string mexAddress = TestContext.Properties["MEX Address"].ToString();
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
            TcpTransportBindingElement bindingElement =
                new TcpTransportBindingElement();
            ServiceEndpointCollection actual;
            string mexAddress = TestContext.Properties["MEX Address"].ToString();
            actual = MetadataHelper_Accessor.GetEndpoints(mexAddress, bindingElement);
            Assert.IsTrue(actual.Count == 1);
            ServiceEndpoint ep = actual[0];
            Assert.AreEqual(typeof(NetTcpBinding), ep.Binding.GetType());
        }
    }
}
