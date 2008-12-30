using System;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeRunner.Security
{
    [TestClass]
    public class Impersonation
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
            public void MyMethod()
            {
                // Impersonation occures here
                WindowsImpersonationContext context =
                    ServiceSecurityContext.Current.WindowsIdentity.Impersonate();
                try
                {
                    // Do work as client
                }
                finally
                {
                    // Revert
                    context.Undo();
                }

                // Equivelent to above... 
                using (context = ServiceSecurityContext.Current.WindowsIdentity.Impersonate())
                {
                    // Do work as client and revert on Dispose
                }
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
        public void ManuallyImpersonateClient()
        {
            using (MyContractClient proxy =
                new MyContractClient(new NetTcpBinding(), address))
            {
                proxy.Open();
                proxy.MyMethod();
                proxy.Close();
            }
        }

        [TestMethod]
        public void ImpersonateClient_AllOperations()
        {
            string address = "net.tcp://localhost:8002/" + Guid.NewGuid().ToString();
            using (ServiceHost host = new ServiceHost(typeof(MyService)))
            {
                host.AddServiceEndpoint(typeof(IMyContract), new NetTcpBinding(), address);
                /* Impersonate all */
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
