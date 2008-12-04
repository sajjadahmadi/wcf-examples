using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ServiceModel.Test
{
    [TestClass]
    public class InProcFactoryTests
    {
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
        public void CreateMultipleInstancesTest2()
        {
            string address = "net.pipe://localhost/";
            ServiceHost<TestService> host = new ServiceHost<TestService>();
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
