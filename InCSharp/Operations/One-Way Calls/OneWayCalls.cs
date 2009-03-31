using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ServiceModel.Examples
{
    [TestClass]
    public class OneWayCalls
    {
        [ServiceContract(SessionMode = SessionMode.Required)]
        interface IMyContract
        {
            [OperationContract()]
            void OneWayCall();

            [OperationContract(IsOneWay = true)]
            void ThrowException();
        }

        [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
        class MyService : IMyContract
        {
            #region IMyContract Members

            public void OneWayCall()
            {
                Trace.WriteLine("OneWayCall");
            }

            public void ThrowException()
            {
                Trace.WriteLine("ThrowException");
                throw new ApplicationException("Exception thrown in a One-Way operation");
            }

            #endregion
        }

        [TestMethod]
        public void OneWayCall()
        {
            string address = "net.pipe://localhost/" + Guid.NewGuid().ToString();
            using (ServiceHost<MyService> host = new ServiceHost<MyService>())
            {
                host.AddServiceEndpoint<IMyContract>(new NetNamedPipeBinding(), address);
                host.OpenTimeout = new TimeSpan(0, 0, 30);
                host.Open();

                IMyContract service = ChannelFactory<IMyContract>.CreateChannel(new NetNamedPipeBinding(), new EndpointAddress(address));
                ICommunicationObject comm = (ICommunicationObject)service;
                Assert.AreEqual(CommunicationState.Created, comm.State);

                service.OneWayCall();
                Assert.AreEqual(CommunicationState.Opened, comm.State);

                // Call causes exception inside service that silently terminates
                service.ThrowException();
                try
                {
                    // Subsequent calls fails 
                    service.OneWayCall();
                    // This is why Juval believes one-way operations on sessionful
                    // services is a bad design.  Here the client doesn't know that 
                    // the serivce is in a faulted state until the next call.
                }
                catch (CommunicationException) { };

                Assert.AreEqual(CommunicationState.Faulted, comm.State);

                try
                {
                    // Cannot close because the connection is in a faulted state.
                    ((ICommunicationObject)service).Close();
                    Assert.Fail("Expected Close() to fail.");
                }
                catch (CommunicationObjectFaultedException) { };
            }
        }
    }
}
