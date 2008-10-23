using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel.Description;

namespace System.ServiceModel.Test
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class ServiceHostTests
    {
        public ServiceHostTests()
        {        }

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
        public void EnableMetadataExchangeTest()
        {
            ServiceHost<TestService> host;

            host = new ServiceHost<TestService>("http://localhost:8080");
            host.AddServiceEndpoint("System.ServiceModel.Test.ITestContract", new WSHttpBinding(), "Test");
            host.EnableMetadataExchange();
            Assert.IsTrue(host.MetadataExchangeEnabled);
            Assert.AreEqual(1, host.Description.Endpoints.Count<ServiceEndpoint>(ep =>
                ep.Contract.ContractType == typeof(IMetadataExchange)));

            host = new ServiceHost<TestService>("http://localhost:8080", "net.tcp://localhost:8081");
            host.AddServiceEndpoint("System.ServiceModel.Test.ITestContract", new WSHttpBinding(), "Test");
            Assert.IsFalse(host.MetadataExchangeEnabled);
            host.EnableMetadataExchange();
            Assert.AreEqual(2, host.Description.Endpoints.Count<ServiceEndpoint>(ep =>
                ep.Contract.ContractType == typeof(IMetadataExchange)));
        }

        [TestMethod]
        public void TypeSafeSingletonTest()
        {

        }
    }
}
