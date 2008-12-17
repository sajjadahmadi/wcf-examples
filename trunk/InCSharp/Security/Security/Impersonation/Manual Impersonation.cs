using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Diagnostics;
using System.Security.Principal;

namespace CodeRunner.Security
{
    [TestClass]
    public class ImpersonationAll
    {
        // Contracts
        [ServiceContract]
        interface IMyContract
        {
            [OperationContract]
            void MyMethod();
        }

        // Service
        [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
        class MyService : IMyContract
        {
            // Impersonation occures here               
            [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
            public void MyMethod()
            {
            }
        }

        // Client
        class MyContractClient : ClientBase<IMyContract>, IMyContract
        {
            public MyContractClient() { }
            public MyContractClient(Binding binding, string address) :
                base(binding, new EndpointAddress(address)) { }

            public void MyMethod()
            { Channel.MyMethod(); }
        }

        [TestMethod]
        public void VerifyImpersonationOnAllOperations()
        {
            string address = "net.tcp://localhost:8002/" + Guid.NewGuid().ToString();
            using (ServiceHost host = new ServiceHost(typeof(MyService)))
            {
                host.AddServiceEndpoint(typeof(IMyContract), new NetTcpBinding(), address);
                // Impersonation VERIFIED here               
                host.Authorization.ImpersonateCallerForAllOperations = true;
                host.Open();

                using (MyContractClient proxy =
                    new MyContractClient(new NetTcpBinding(), address))
                {
                    proxy.Open();
                    proxy.MyMethod();
                    proxy.Close();
                }

                host.Close();
            }
        }
    }
}
