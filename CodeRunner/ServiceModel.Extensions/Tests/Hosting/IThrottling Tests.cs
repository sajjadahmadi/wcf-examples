using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.Serialization;

namespace System.ServiceModel.Examples
{
    [TestClass]
    public class IThrottlingTests
    {
        // Contracts
        [DataContract]
        public class ThrottleInfo
        {
            public ThrottleInfo(ServiceThrottle throttle) {
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
        public interface IThrottlingInformation
        {
            [OperationContract]
            ThrottleInfo GetThrottleInfo();
        }

        // Service
        [ServiceBehavior]
        public class ThrottledService : IThrottlingInformation
        {
            public ThrottleInfo GetThrottleInfo() { return new ThrottleInfo(MyThrottle); }

            private ServiceThrottle MyThrottle {
                get {
                    ServiceThrottle serviceThrottle;
                    ChannelDispatcher dispatcher =
                        OperationContext.Current.Host.ChannelDispatchers[0] as ChannelDispatcher;
                    serviceThrottle = dispatcher.ServiceThrottle;
                    return serviceThrottle;
                }
            }
        }


        [TestMethod]
        public void TestSetThrottle() {
            NetNamedPipeBinding binding = new NetNamedPipeBinding();
            string address = "net.pipe://localhost/" + Guid.NewGuid().ToString();

            using (ServiceHost<ThrottledService> host = new ServiceHost<ThrottledService>(address)) {
                host.AddServiceEndpoint<IThrottlingInformation>(binding, "");
                host.SetThrottle(12, 34, 56);
                host.Open();

                IThrottlingInformation service = ChannelFactory<IThrottlingInformation>.CreateChannel(
                    binding,
                    new EndpointAddress(address));

                using (service as IDisposable) {
                    ThrottleInfo info = service.GetThrottleInfo();
                    Assert.AreEqual(12, info.MaxConcurrentCalls);
                    Assert.AreEqual(34, info.MaxConcurrentSessions);
                    Assert.AreEqual(56, info.MaxConcurrentInstances);
                }
            }
        }

    }
}
