using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel.Description;

namespace System.ServiceModel.Test
{

    [TestClass]
    public class ServiceHostTests
    {
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

        [TestMethod]
        public void EnableMetadataExchangeTest()
        {
            ServiceHost<TestService> host;

            host = new ServiceHost<TestService>("http://localhost:8080");
            host.AddServiceEndpoint(typeof(ITestContract), new WSHttpBinding(), "Test");
            host.EnableMetadataExchange();
            Assert.IsTrue(host.MetadataExchangeEnabled);
            Assert.AreEqual(1, host.Description.Endpoints.Count<ServiceEndpoint>(ep =>
                ep.Contract.ContractType == typeof(IMetadataExchange)));

            host = new ServiceHost<TestService>("http://localhost:8080", "net.tcp://localhost:8081");
            host.AddServiceEndpoint(typeof(ITestContract), new WSHttpBinding(), "Test");
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
