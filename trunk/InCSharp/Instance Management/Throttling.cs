using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using System.Threading;

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
        interface IMyContract
        {
            [OperationContract]
            ThrottleInfo GetThrottleInfo();
        }

        // Service
        [ServiceBehavior]
        class ThrottledService : IMyContract
        {
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

        // Client
        class MyContractClient : ClientBase<IMyContract>, IMyContract
        {
            public MyContractClient(Binding binding, string address)
                : base(binding, new EndpointAddress(address))
            { }

            public ThrottleInfo GetThrottleInfo()
            {
                return Channel.GetThrottleInfo();
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
                throttle.MaxConcurrentCalls = 12;
                throttle.MaxConcurrentSessions = 34;
                throttle.MaxConcurrentInstances = 56;
                host.Description.Behaviors.Add(throttle);
                host.AddServiceEndpoint(typeof(IMyContract), binding, "");
                host.Open();

                // Throttle read here
                using (MyContractClient proxy = new MyContractClient(binding, address))
                {
                    ThrottleInfo info = proxy.GetThrottleInfo();
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
            string address = "net.pipe://localhost/" + Guid.NewGuid().ToString();

            using (ServiceHost host = new ServiceHost(typeof(ThrottledService), new Uri(address)))
            {
                // Throttle set here
                ServiceThrottlingBehavior throttle = new ServiceThrottlingBehavior();
                throttle.MaxConcurrentSessions = 1;
                host.Description.Behaviors.Add(throttle);
                host.AddServiceEndpoint(typeof(IMyContract), new NetNamedPipeBinding(), "");
                host.Open();
                Assert.AreEqual(CommunicationState.Opened, host.State);

                // Try to start two sessions
                using (MyContractClient proxy1 = new MyContractClient(new NetNamedPipeBinding(), address))
                {
                    proxy1.Open();

                    NetNamedPipeBinding binding = new NetNamedPipeBinding() { OpenTimeout = new TimeSpan(0, 0, 1) };
                    MyContractClient proxy2 = new MyContractClient(binding, address);
                    proxy2.Open(); // Should timeout
                }
            }
        }

    }
}
