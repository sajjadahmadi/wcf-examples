using System;
using System.Diagnostics;
using System.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WcfExamples
{
    [TestClass]
    public class OneWayCallsAndExceptions
    {
        [ServiceContract(SessionMode = SessionMode.Required)]
        interface IMyContract
        {
            [OperationContract(IsOneWay = true)]
            void OneWayWithError();

            [OperationContract]
            void NormalMethod();
        }

        [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
        [DebuggerNonUserCode]
        class MyService : IMyContract
        {
            public void OneWayWithError()
            {
                throw new ApplicationException("Exception thrown in a One-Way operation");
            }

            public void NormalMethod()
            { }
        }

        class MyContractClient : ClientBase<IMyContract>, IMyContract
        {
            public MyContractClient(string address)
                : base(new NetNamedPipeBinding(), new EndpointAddress(address))
            { }

            public void OneWayWithError()
            {
                Channel.OneWayWithError();
            }

            public void NormalMethod()
            {
                Channel.NormalMethod();
            }
        }


        [TestMethod]
        public void OneWayExceptions()
        {
            string address = "net.pipe://localhost/" + Guid.NewGuid().ToString();
            using (ServiceHost<MyService> host = new ServiceHost<MyService>())
            {
                host.AddServiceEndpoint<IMyContract>(new NetNamedPipeBinding(), address);
                host.Open();

                MyContractClient proxy = new MyContractClient(address);
                Assert.AreEqual(CommunicationState.Created, proxy.State);

                proxy.NormalMethod();
                Assert.AreEqual(CommunicationState.Opened, proxy.State);

                // This call causes exception inside service that silently terminates
                // and faults the channel
                proxy.OneWayWithError();
                // Here the CLIENT doesn't know that the channel is faulted 
                // until the next non-one-way call.
                Assert.AreEqual(CommunicationState.Opened, proxy.State);

                try
                {
                    // Subsequent calls fails 
                    proxy.NormalMethod();
                }
                catch (CommunicationException)
                {
                    // Juval believes one-way operations on sessionful
                    // services is a bad design.  
                };

                Assert.AreEqual(CommunicationState.Faulted, proxy.State);

                try
                {
                    // Cannot close because the connection is in a faulted state.
                    proxy.Close();
                    Assert.Fail("Expected Close() to fail.");
                }
                catch (CommunicationObjectFaultedException) { };
            }
        }
    }
}
