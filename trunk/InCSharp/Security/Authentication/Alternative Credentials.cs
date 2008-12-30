using System;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeRunner.Security
{
    [TestClass]
    public class AlternativeCredentials
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
                return;
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
        public void UseAlternativeCredentials()
        {
            using (MyContractClient proxy =
                new MyContractClient(new NetTcpBinding(), address))
            {
                NetworkCredential credentials = new NetworkCredential();
                credentials.Domain = "MyDomain";
                credentials.UserName = "MyUsername";
                credentials.Password = "MyPassword";
                //proxy.ClientCredentials.Windows.ClientCredential = credentials;

                proxy.Open();
                proxy.MyMethod();
                proxy.Close();
            }
        }
    }
}
