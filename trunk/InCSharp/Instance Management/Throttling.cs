using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.Serialization;

namespace System.ServiceModel.Examples
{
    [TestClass]
    public class Throttling
    {
        [DataContract]
        class ThrottleInfo
        {
            public ThrottleInfo(ServiceThrottle throttle)
            {
                MaxConcurrentCalls = throttle.MaxConcurrentCalls;
                MaxConcurrentInstances = throttle.MaxConcurrentInstances;
                MaxConcurrentSessions = throttle.MaxConcurrentSessions;
            }

            [DataMember]
            public int MaxConcurrentCalls { get; set; }
            [DataMember]
            public int MaxConcurrentInstances { get; set; }
            [DataMember]
            public int MaxConcurrentSessions { get; set; }
        }

        [ServiceContract]
        interface IThrottlingInformation
        {
            [OperationContract]
            ThrottleInfo GetThrottleInfo();
        }

        [ServiceContract]
        interface IMyContract
        {
            [OperationContract]
            bool MyMethod();
        }

        // Service
        [ServiceBehavior]
        class ThrottledService : IThrottlingInformation, IMyContract
        {
            public bool MyMethod() { return true; }

            public ThrottleInfo GetThrottleInfo() { return new ThrottleInfo(MyThrottle); }

            private ServiceThrottle MyThrottle
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

        }

        [TestMethod]
        public void ReadThrottlingValues()
        {
            NetNamedPipeBinding binding = new NetNamedPipeBinding();
            string address = "net.pipe://localhost/" + Guid.NewGuid().ToString();

            using (ServiceHost host = new ServiceHost(typeof(ThrottledService), new Uri(address)))
            {
                // Throttle set here
                ServiceThrottlingBehavior throttle = new ServiceThrottlingBehavior();
                host.Description.Behaviors.Add(throttle);
                host.AddServiceEndpoint(typeof(IThrottlingInformation), binding, "");
                host.Open();

                // Throttle read here
                IThrottlingInformation service = ChannelFactory<IThrottlingInformation>.CreateChannel(
                    binding,
                    new EndpointAddress(address));
                using (service as IDisposable)
                {
                    ThrottleInfo info = service.GetThrottleInfo();
                    Assert.AreEqual(12, info.MaxConcurrentCalls);
                    Assert.AreEqual(34, info.MaxConcurrentSessions);
                    Assert.AreEqual(56, info.MaxConcurrentInstances);
                }
            }
        }

        [TestMethod]
        [ExpectedException(
            typeof(TimeoutException),
            "The open operation did not complete within the allotted timeout of 00:00:01. The time allotted to this operation may have been a portion of a longer timeout.")]
        public void MaxSessions()
        {
            NetNamedPipeBinding binding = new NetNamedPipeBinding();
            string address = "net.pipe://localhost/" + Guid.NewGuid().ToString();

            using (ServiceHost host = new ServiceHost(typeof(ThrottledService), new Uri(address)))
            {
                // Throttle set here
                ServiceThrottlingBehavior throttle = new ServiceThrottlingBehavior();
                throttle.MaxConcurrentSessions = 1;
                host.Description.Behaviors.Add(throttle);
                host.AddServiceEndpoint(typeof(IMyContract), binding, "");
                host.Open();
                Assert.AreEqual(CommunicationState.Opened, host.State);

                IMyContract proxy1 = ChannelFactory<IMyContract>.CreateChannel(
                    new NetNamedPipeBinding(),
                    new EndpointAddress(address));

                using (proxy1 as IDisposable)
                {
                    Assert.AreEqual(true, proxy1.MyMethod());

                    IMyContract proxy2;
                    proxy2 = ChannelFactory<IMyContract>.CreateChannel(
                        new NetNamedPipeBinding() { SendTimeout = new TimeSpan(0, 0, 1) },
                        new EndpointAddress(address));
                    Assert.AreEqual(true, proxy2.MyMethod());
                    Assert.Fail("Expected SendTimeout exception.");
                }
            }

        }

        // TODO: Fix broken test!
        [TestMethod]
        public void BindingMaxConnections()
        {
            string address = "net.pipe://localhost/" + Guid.NewGuid().ToString();
            ServiceHost<ThrottledService> host = new ServiceHost<ThrottledService>(address);
            NetNamedPipeBinding binding = new NetNamedPipeBinding();
            host.OpenTimeout = new TimeSpan(0, 0, 30);
            binding.MaxConnections = 1;
            host.AddServiceEndpoint(typeof(IMyContract), binding, "");
            host.Open();

            IMyContract service1 = ChannelFactory<IMyContract>.CreateChannel(
                new NetNamedPipeBinding(),
                new EndpointAddress(address));
            Assert.AreEqual(true, service1.MyMethod());

            try
            {
                IMyContract service2 = ChannelFactory<IMyContract>.CreateChannel(
                    new NetNamedPipeBinding() { SendTimeout = new TimeSpan(0, 0, 2) },
                    new EndpointAddress(address));
                Assert.AreEqual(true, service2.MyMethod());
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
