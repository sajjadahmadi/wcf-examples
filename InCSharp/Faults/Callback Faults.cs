using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;

namespace System.ServiceModel.Examples
{
    [TestClass]
    public class CallbackFaults
    {
        // Contracts
        [ServiceContract(CallbackContract = typeof(IMyContractCallback))]
        interface IMyContract
        {
            [OperationContract]
            void MyMethod();
        }

        interface IMyContractCallback
        {
            [OperationContract]
            void OnCallback();
        }

        // Service
        [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
        class MyService : IMyContract
        {
            public void MyMethod()
            {
                IMyContractCallback callback =
                    OperationContext.Current.GetCallbackChannel<IMyContractCallback>();
                callback.OnCallback();
            }
        }

        // Client
        class MyContractClient : DuplexClientBase<IMyContract, IMyContractCallback>, IMyContract
        {
            public MyContractClient(
                IMyContractCallback callback,
                Binding binding,
                string remoteAddress) :
                base(callback, binding, new EndpointAddress(remoteAddress)) { }

            public void MyMethod()
            {
                Channel.MyMethod();
            }
        }

        class MyContractCallback : IMyContractCallback
        {
            public void OnCallback()
            {
                try
                {
                    throw new ApplicationException("Simulated application exception.");
                }
                catch (ApplicationException ex)
                {
                    throw new FaultException(ex.Message);
                }
            }
        }

        #region Host
        static NetNamedPipeBinding binding;
        static string address = "net.pipe://localhost/" + Guid.NewGuid().ToString();
        static ServiceHost<MyService> host;

        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            binding = new NetNamedPipeBinding();
            host = new ServiceHost<MyService>();
            host.AddServiceEndpoint<IMyContract>(binding, address);
            host.Open();
        }

        // Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup()]
        public static void MyClassCleanup()
        {
            host.Close();
        }
        #endregion

        [TestMethod]
        [ExpectedException(typeof(FaultException), "Simulated application exception.")]
        public void CallbackFault()
        {
            MyContractCallback callback = new MyContractCallback();

            MyContractClient client = new MyContractClient(callback, binding, address);
            client.Open();
            try
            {
                client.MyMethod();
            }
            finally
            {
                client.Close();
            }
        }
    }
}
