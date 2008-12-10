using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeRunner.ServiceModel.Examples
{
    [TestClass]
    public class FaultExceptionTests
    {
        [ServiceContract]
        interface IMyContract
        {
            [OperationContract]
            [FaultContract(typeof(MyFault))]
            void ThrowTypedFault();

            [OperationContract]
            void ThrowUntypedFault();
        }

        [DataContract]
        class MyFault
        {
            private string description;

            public MyFault(string description)
            { this.description = description; }

            [DataMember]
            public string Description
            {
                get { return description; }
                set { description = value; }
            }
        }

        /* Service */
        class MyService : IMyContract
        {
            public void ThrowTypedFault()
            {
                try
                {
                    throw new ApplicationException("Some application error.");
                }
                catch (ApplicationException ex)
                {
                    MyFault fault = new MyFault(ex.Message);
                    throw new FaultException<MyFault>(fault);
                }
            }

            public void ThrowUntypedFault()
            { throw new FaultException("Untyped Fault."); }
        }

        /* Client */
        class MyContractClient : ClientBase<IMyContract>, IMyContract
        {
            public MyContractClient() { }
            public MyContractClient(Binding binding, string remoteAddress) :
                base(binding, new EndpointAddress(remoteAddress)) { }

            public void ThrowTypedFault()
            { Channel.ThrowTypedFault(); }

            public void ThrowUntypedFault()
            { Channel.ThrowUntypedFault(); }
        }

        #region Additional test attributes
        static NetNamedPipeBinding binding;
        static string address = "net.pipe://localhost/" + Guid.NewGuid().ToString();
        static ServiceHost<MyService> host;

        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            binding = new NetNamedPipeBinding();
            host = new ServiceHost<MyService>();
            host.AddServiceEndpoint<IMyContract>(binding, address);
            host.Open();
        }

        [ClassCleanup()]
        public static void MyClassCleanup()
        {
            host.Close();
        }
        #endregion

        [TestMethod]
        [ExpectedException(typeof(FaultException<MyFault>))]
        public void TypedFault()
        {
            MyContractClient client = new MyContractClient(binding, address);
            client.ThrowTypedFault();
            client.Close();
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException))]
        public void UntypedFault()
        {
            MyContractClient client = new MyContractClient(binding, address);
            client.ThrowUntypedFault();
            client.Close();
        }

    }
}
