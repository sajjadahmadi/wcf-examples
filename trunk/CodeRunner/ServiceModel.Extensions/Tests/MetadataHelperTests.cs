using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ServiceModel.Examples
{
    [TestClass()]
    public class MetadataHelperTests
    {
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
