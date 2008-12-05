using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ServiceModel.Extension.Test
{
    /// <summary>
    /// Summary description for BindingRequirementAttribute
    /// </summary>
    [TestClass]
    public class BindingRequirementAttributeTests
    {
        [ServiceContract]
        public interface ITestContract2
        {
            [OperationContract]
            [TransactionFlow(TransactionFlowOption.Allowed)]
            string MyOperation();
        }

        [BindingRequirement(TransactionFlowRequired = true)]
        class TestService2 : ITestContract2
        {
            public string MyOperation()
            {
                return "MyResult";
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void InvalidBindingType()
        {
            // Expect this to throw an InvalidOperationException, 
            // but it throws a CommunicationObjectFaultedException
            using (ServiceHost host = new ServiceHost(typeof(TestService2)))
            {
                BasicHttpBinding binding = new BasicHttpBinding();  // Does not support transactions
                string address = "http://localhost:8080/" + Guid.NewGuid().ToString();
                host.AddServiceEndpoint(typeof(ITestContract2), binding, address);
                host.Open();
            }
        }

        [TestMethod]
        //[ExpectedException(typeof(InvalidOperationException))]
        public void TransactionFlowDisabled()
        {
            // Expect this to throw an InvalidOperationException, 
            // but it throws a CommunicationObjectFaultedException
            using (ServiceHost<TestService2> host = new ServiceHost<TestService2>())
            {
                NetNamedPipeBinding binding = new NetNamedPipeBinding();
                //binding.TransactionFlow = true;
                string address = "net.pipe://localhost/" + Guid.NewGuid().ToString();
                host.AddServiceEndpoint(typeof(ITestContract2), binding, address);
                host.Open();
            }
        }
    }
}
