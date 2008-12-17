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
    public class DeclarativeImpersonation
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
            [OperationBehavior(Impersonation = ImpersonationOption.Required)] 
            public void MyMethod()
            {
                // Do work as client and auto-revert
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

        #region Host
        static string address = "net.tcp://localhost:8001/" + Guid.NewGuid().ToString();
        static ServiceHost host;

        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            host = new ServiceHost(typeof(MyService));
            host.AddServiceEndpoint(typeof(IMyContract), new NetTcpBinding(), address);
            host.Open();
        }

        [ClassCleanup()]
        public static void MyClassCleanup()
        {
            if (host.State == CommunicationState.Opened)
                host.Close();
        }
        #endregion

        [TestMethod]
        public void DeclarativelyImpersonateClient()
        {
            using (MyContractClient proxy =
                new MyContractClient(new NetTcpBinding(), address))
            {
                proxy.Open();
                proxy.MyMethod();
                proxy.Close();
            }
        }
    }
}
