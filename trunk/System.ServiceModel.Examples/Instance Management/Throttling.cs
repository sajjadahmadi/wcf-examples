using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Description;

namespace System.ServiceModel.Examples
{
    [TestClass]
    public class Throttling
    {
        [ServiceContract]
        public interface IThrottlingInformation
        {
            [OperationContract]
            int ReadMaxConcurrentCalls();
            [OperationContract]
            int ReadMaxConcurrentSessions();
            [OperationContract]
            int ReadMaxConcurrentInstances();
        }

        [ServiceContract]
        public interface IPingService
        {
            [OperationContract]
            bool Ping();
        }

        [ServiceBehavior]
        public class ThrottledService : IThrottlingInformation, IPingService
        {

            #region IThrottlingInformation

            public int ReadMaxConcurrentCalls()
            { return Throttle.MaxConcurrentCalls; }
            public int ReadMaxConcurrentSessions()
            { return Throttle.MaxConcurrentSessions; }
            public int ReadMaxConcurrentInstances()
            { return Throttle.MaxConcurrentInstances; }

            #endregion IThrottlingInformation

            private ServiceThrottle Throttle
            {
                get
                {
                    ServiceThrottle serviceThrottle;
                    ChannelDispatcher dispatcher =
                        OperationContext.Current.Host.ChannelDispatchers[0] as ChannelDispatcher;
                    serviceThrottle = dispatcher.ServiceThrottle;
                    return serviceThrottle;
                }
            }

            #region IPingService Members

            public bool Ping()
            { return true; }

            #endregion
        }


        [TestMethod]
        public void ReadThrottlingValues()
        {
            NetNamedPipeBinding binding = new NetNamedPipeBinding();
            ServiceThrottlingBehavior throttle = new ServiceThrottlingBehavior();
            throttle.MaxConcurrentSessions = 1;
            string address = "http://localhost:8080/";

            using (ServiceHost<ThrottledService> host = new ServiceHost<ThrottledService>(address))
            {
                host.AddServiceEndpoint(typeof(IThrottlingInformation), new WSHttpBinding(), "");
                host.SetThrottle(12, 34, 56);
                host.Open();

                IThrottlingInformation service = ChannelFactory<IThrottlingInformation>.CreateChannel(
                    new WSHttpBinding(),
                    new EndpointAddress(address));

                Assert.AreEqual(12, service.ReadMaxConcurrentCalls());
                Assert.AreEqual(34, service.ReadMaxConcurrentSessions());
                Assert.AreEqual(56, service.ReadMaxConcurrentInstances());

                ((ICommunicationObject)service).Close();
            }
        }

        [TestMethod]
        public void MaxSessions()
        {
            NetNamedPipeBinding binding = new NetNamedPipeBinding();
            ServiceThrottlingBehavior throttle = new ServiceThrottlingBehavior();
            throttle.MaxConcurrentSessions = 1;
            string address = "net.pipe://localhost/";

            using (ServiceHost<ThrottledService> host = new ServiceHost<ThrottledService>(address))
            {
                host.AddServiceEndpoint(typeof(IPingService), binding, "");
                host.SetThrottle(throttle);
                host.Open();

                Assert.AreEqual(CommunicationState.Opened, host.State);
                
                IPingService service1 = ChannelFactory<IPingService>.CreateChannel(
                    new NetNamedPipeBinding(),
                    new EndpointAddress(address));
                Assert.AreEqual(true, service1.Ping());
                
                IPingService service2;
                try
                {
                    service2 = ChannelFactory<IPingService>.CreateChannel(
                        new NetNamedPipeBinding() { SendTimeout = new TimeSpan(0, 0, 1) },
                        new EndpointAddress(address));
                    Assert.AreEqual(true, service2.Ping());
                    ((ICommunicationObject)service2).Close();
                    Assert.Fail("Expected SendTimeout exception.");
                }
                catch (TimeoutException) { }
                finally
                {
                    ((ICommunicationObject)service1).Close();
                }

                // Service1 is Closed, so Service2 should now work
                service2 = ChannelFactory<IPingService>.CreateChannel(
                    new NetNamedPipeBinding(),
                    new EndpointAddress(address));
                Assert.AreEqual(true, service2.Ping());
                ((ICommunicationObject)service2).Close();
            }
        }

        [TestMethod]
        public void BindingMaxConnections()
        {
            string address = "net.pipe://localhost/";
            ServiceHost<ThrottledService> host = new ServiceHost<ThrottledService>(address);
            NetNamedPipeBinding binding = new NetNamedPipeBinding();
            binding.MaxConnections = 1;
            host.AddServiceEndpoint(typeof(IPingService), binding, "");
            host.Open();

            IPingService service1 = ChannelFactory<IPingService>.CreateChannel(
                new NetNamedPipeBinding(),
                new EndpointAddress(address));
            Assert.AreEqual(true, service1.Ping());

            try
            {
                IPingService service2 = ChannelFactory<IPingService>.CreateChannel(
                    new NetNamedPipeBinding() { SendTimeout = new TimeSpan(0, 0, 2) },
                    new EndpointAddress(address));
                Assert.AreEqual(true, service2.Ping());
                ((ICommunicationObject)service2).Close();
                Assert.Fail("Expected SendTimeout exception.");
            }
            catch (TimeoutException) { }
            finally
            {
                ((ICommunicationObject)service1).Close();
                host.Close();
            }
        }
    }
}
