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
        public void EnableMetadataExchange()
        {
            ServiceHost<TestService> host;
            using (host = new ServiceHost<TestService>("http://localhost:8080"))
            {
                host.AddServiceEndpoint(typeof(ITestContract), new WSHttpBinding(), "Test");
                host.EnableMetadataExchange();
                Assert.IsTrue(host.MetadataExchangeEnabled);
                Assert.AreEqual(1, host.Description.Endpoints.Count<ServiceEndpoint>(ep =>
                    ep.Contract.ContractType == typeof(IMetadataExchange)));
            }
        }

        [TestMethod]
        public void EnableMetadataExchange_MultipleBaseAddresses()
        {
            ServiceHost<TestService> host;
            using (host = new ServiceHost<TestService>("http://localhost:8080", "net.tcp://localhost:8081"))
            {
                host.AddServiceEndpoint(typeof(ITestContract), new WSHttpBinding(), "Test");
                Assert.IsFalse(host.MetadataExchangeEnabled);
                host.EnableMetadataExchange();
                Assert.AreEqual(2, host.Description.Endpoints.Count<ServiceEndpoint>(ep =>
                    ep.Contract.ContractType == typeof(IMetadataExchange)));
            }
        }

        [TestMethod]
        public void CreateMultipleChannelProxies()
        {
            string address = "net.pipe://localhost/";
            using (ServiceHost<TestService> host = new ServiceHost<TestService>())
            {
                host.AddServiceEndpoint<ITestContract>(new NetNamedPipeBinding(), address);
                host.Open();
                ITestContract proxy1 = host.CreateChannel<ITestContract>(new NetNamedPipeBinding(), address);
                ITestContract proxy2 = host.CreateChannel<ITestContract>(new NetNamedPipeBinding(), address);
                Assert.AreNotSame(proxy1, proxy2);
                Assert.AreEqual<string>("MyResult", proxy1.MyOperation());
                Assert.AreEqual<string>("MyResult", proxy2.MyOperation());
                ((ICommunicationObject)proxy1).Close();
                ((ICommunicationObject)proxy2).Close();
            }
        }

        [TestMethod]
        [Ignore]
        public void TypeSafeSingleton()
        {
            Assert.Inconclusive("This test was never written.  Does it need to be here?");
        }
    }
}
