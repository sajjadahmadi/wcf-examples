using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeRunner.Security
{
    [TestClass]
    public class VerifyImpersonationAll
    {
        // Contracts
        [ServiceContract]
        interface IMyContract
        {
            [OperationContract]
            void MyMethod();
        }

        // Service
        class MyService : IMyContract
        {
            [OperationBehavior(Impersonation = ImpersonationOption.NotAllowed)]
            public void MyMethod()
            {
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException),
            "The service operation 'MyMethod' that belongs to the contract with the 'IMyContract' name and the 'http://tempuri.org/' namespace does not allow impersonation.")]
        public void VerifyImpersonationOnAllOperations()
        {
            ServiceHost host = new ServiceHost(typeof(MyService));

            // Verifies that there are no operations with IpersonationOption.NotAllowed (default)
            host.Authorization.ImpersonateCallerForAllOperations = true;

            host.Open();
        }
    }
}
