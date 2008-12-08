using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ServiceModel.Test
{
    [TestClass()]
    public class MetadataHelperTests
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
        public void MyTestInitialize()
        {
        }
        //
        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
        }

        #endregion

        #region Service
        [ServiceContract(Name = "MyServiceName", Namespace = "MyServiceNamespace")]
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
        /// A test for SupportsContract
        ///</summary>
        [TestMethod()]
        public void SupportsContractOverload1()
        {
            ServiceHost<TestService> host;
            using (host = new ServiceHost<TestService>("net.pipe://localhost/" + Guid.NewGuid().ToString()))
            {
                ServiceMetadataBehavior behavior = new ServiceMetadataBehavior();
                host.Description.Behaviors.Add(behavior);

                host.AddServiceEndpoint<ITestContract>(new NetNamedPipeBinding(), "");
                ServiceEndpoint mexEndpoint = host.AddServiceEndpoint<IMetadataExchange>(MetadataExchangeBindings.CreateMexNamedPipeBinding(), "mex");
                host.Open();

                Assert.IsTrue(MetadataHelper.SupportsContract(mexEndpoint.Address.ToString(), typeof(ITestContract)));
            }
        }

        /// <summary>
        /// A test for SupportsContract
        ///</summary>
        [TestMethod()]
        public void SupportsContractOverload2()
        {
            ServiceHost<TestService> host;
            using (host = new ServiceHost<TestService>("net.pipe://localhost/" + Guid.NewGuid().ToString()))
            {
                ServiceMetadataBehavior behavior = new ServiceMetadataBehavior();
                host.Description.Behaviors.Add(behavior);

                host.AddServiceEndpoint<ITestContract>(new NetNamedPipeBinding(), "");
                ServiceEndpoint mexEndpoint = host.AddServiceEndpoint<IMetadataExchange>(MetadataExchangeBindings.CreateMexNamedPipeBinding(), "mex");
                host.Open();

                Assert.IsTrue(MetadataHelper.SupportsContract(mexEndpoint.Address.ToString(), "MyServiceNamespace", "MyServiceName"));
            }
        }

        /// <summary>
        /// A test for GetEndpoints
        ///</summary>
        [TestMethod()]
        public void GetPipeEndpoints()
        {
            ServiceHost<TestService> host;
            using (host = new ServiceHost<TestService>("net.pipe://localhost/" + Guid.NewGuid().ToString()))
            {
                ServiceMetadataBehavior behavior = new ServiceMetadataBehavior();
                host.Description.Behaviors.Add(behavior);

                host.AddServiceEndpoint<ITestContract>(new NetNamedPipeBinding(), "1");
                host.AddServiceEndpoint<ITestContract>(new NetNamedPipeBinding(), "2");
                ServiceEndpoint mexEndpoint = host.AddServiceEndpoint<IMetadataExchange>(MetadataExchangeBindings.CreateMexNamedPipeBinding(), "mex");
                host.Open();

                ServiceEndpointCollection endpoints = MetadataHelper.GetEndpoints(mexEndpoint.Address.ToString());
                Assert.AreEqual(2, endpoints.Count);
            }
        }

        /// <summary>
        /// A test for GetEndpoints
        ///</summary>
        [TestMethod()]
        public void GetTcpEndpoints()
        {
            ServiceHost<TestService> host;
            using (host = new ServiceHost<TestService>("net.tcp://localhost/" + Guid.NewGuid().ToString()))
            {
                ServiceMetadataBehavior behavior = new ServiceMetadataBehavior();
                host.Description.Behaviors.Add(behavior);

                host.AddServiceEndpoint<ITestContract>(new NetTcpBinding(), "1");
                host.AddServiceEndpoint<ITestContract>(new NetTcpBinding(), "2");
                ServiceEndpoint mexEndpoint = host.AddServiceEndpoint<IMetadataExchange>(MetadataExchangeBindings.CreateMexTcpBinding(), "mex");
                host.Open();

                ServiceEndpointCollection endpoints = MetadataHelper.GetEndpoints(mexEndpoint.Address.ToString());
                Assert.AreEqual(2, endpoints.Count);
            }
        }

        /// <summary>
        /// A test for GetEndpoints
        ///</summary>
        [TestMethod()]
        public void GetHttpEndpoints()
        {
            ServiceHost<TestService> host;
            using (host = new ServiceHost<TestService>("http://localhost/" + Guid.NewGuid().ToString()))
            {
                ServiceMetadataBehavior behavior = new ServiceMetadataBehavior();
                host.Description.Behaviors.Add(behavior);

                host.AddServiceEndpoint<ITestContract>(new BasicHttpBinding(), "1");
                host.AddServiceEndpoint<ITestContract>(new WSHttpBinding(), "2");
                ServiceEndpoint mexEndpoint = host.AddServiceEndpoint<IMetadataExchange>(MetadataExchangeBindings.CreateMexHttpBinding(), "mex");
                host.Open();

                ServiceEndpointCollection endpoints = MetadataHelper.GetEndpoints(mexEndpoint.Address.ToString());
                Assert.AreEqual(2, endpoints.Count);
            }
        }

        /// <summary>
        /// A test for GetEndpoints
        ///</summary>
        [TestMethod()]
        [Ignore]    // TODO: Fix. May need certificate.
        public void GetHttpsEndpoints()
        {
            ServiceHost<TestService> host;
            using (host = new ServiceHost<TestService>("https://localhost/" + Guid.NewGuid().ToString()))
            {
                ServiceMetadataBehavior behavior = new ServiceMetadataBehavior();
                host.Description.Behaviors.Add(behavior);

                host.AddServiceEndpoint<ITestContract>(new BasicHttpBinding(BasicHttpSecurityMode.Transport), "1");
                host.AddServiceEndpoint<ITestContract>(new BasicHttpBinding(BasicHttpSecurityMode.Transport), "2");
                ServiceEndpoint mexEndpoint = host.AddServiceEndpoint<IMetadataExchange>(MetadataExchangeBindings.CreateMexHttpsBinding(), "mex");
                host.Open();

                ServiceEndpointCollection endpoints = MetadataHelper.GetEndpoints(mexEndpoint.Address.ToString());
                Assert.AreEqual(2, endpoints.Count);
            }
        }
    }
}
