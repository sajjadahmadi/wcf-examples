using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeRunner.ServiceModel.Examples
{
    [TestClass]
    public class FaultExceptionDetail
    {
        // Contracts
        [ServiceContract]
        interface IMyContract
        {
            [OperationContract]
            void ThrowClrException();
        }

        // Service
        [ServiceBehavior(IncludeExceptionDetailInFaults = true)] // ExpectedException
        class MyService : IMyContract
        {
            public void ThrowClrException()
            {
                throw new NotImplementedException();
            }
        }

        // Client
        class MyContractClient : ClientBase<IMyContract>, IMyContract
        {
            public MyContractClient() { }
            public MyContractClient(Binding binding, string remoteAddress) :
                base(binding, new EndpointAddress(remoteAddress)) { }

            public void ThrowClrException()
            { Channel.ThrowClrException(); }
        }

        #region Host
        static NetNamedPipeBinding binding;
        static string address = "net.pipe://localhost/" + Guid.NewGuid().ToString();
        static ServiceHost<MyService> host;

        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            binding = new NetNamedPipeBinding();
            host = new ServiceHost<MyService>();
            host.AddServiceEndpoint<IMyContract>(binding, address);
            // host.IncludeExceptionDetailInFaults = true;
            host.Open();
        }

        [ClassCleanup()]
        public static void MyClassCleanup()
        {
            host.Close();
        }
        #endregion

        [TestMethod]
        [ExpectedException(typeof(FaultException<ExceptionDetail>))]
        public void ExceptionDetail()
        {
            MyContractClient client = new MyContractClient(binding, address);
            try
            {
                client.ThrowClrException();
            }
            catch (FaultException<ExceptionDetail> ex)
            {
                Assert.AreEqual("System.NotImplementedException", ex.Detail.Type);
                throw;
            }
        }
    }
}
