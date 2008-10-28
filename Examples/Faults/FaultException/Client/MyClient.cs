using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Client.MyServiceReference;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Client
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        [ExpectedException(typeof(FaultException<FaultType>))]
        public void TypedFaultException()
        {
            MyContractClient client = new MyContractClient();
            client.ThrowTypedFault();
            client.Close();
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void UntypedFaultException()
        {
            MyContractClient client = new MyContractClient();
            client.ThrowUntypedFault();
            client.Close();
        }

    }
}
