using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel;
using System.Diagnostics;

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
            string address = "net.pipe://localhost/";
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
