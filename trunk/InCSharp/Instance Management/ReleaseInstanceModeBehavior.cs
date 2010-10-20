using System.ServiceModel.Channels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ServiceModel.Examples
{
    [ServiceContract]
    interface IMyCounter
    {
        [OperationContract]
        int IncrementCounterReleaseNone();

        [OperationContract]
        int IncrementCounterReleaseBefore();

        [OperationContract]
        int IncrementCounterReleaseAfter();

        [OperationContract]
        int IncrementCounterReleaseBeforeAndAfter();
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, IncludeExceptionDetailInFaults=true)]
    internal class MyService : IMyCounter
    {
        public int Count;

        public MyService()
            : this(0)
        {
        }

        public MyService(int initialCount)
        {
            this.Count = initialCount;
        }

        [OperationBehavior(ReleaseInstanceMode=ReleaseInstanceMode.None)]
        public int IncrementCounterReleaseNone()
        {
            return ++Count;
        }

        [OperationBehavior(ReleaseInstanceMode = ReleaseInstanceMode.BeforeCall)]
        public int IncrementCounterReleaseBefore()
        {
            return ++Count;
        }

        [OperationBehavior(ReleaseInstanceMode = ReleaseInstanceMode.AfterCall)]
        public int IncrementCounterReleaseAfter()
        {
            return ++Count;
        }

        [OperationBehavior(ReleaseInstanceMode = ReleaseInstanceMode.BeforeAndAfterCall)]
        public int IncrementCounterReleaseBeforeAndAfter()
        {
            return ++Count;
        }
    }

    [TestClass]
    public class ReleaseInstanceModeBehavior
    {
        [TestMethod]
        public void ReleaseInstanceModeNoneTest()
        {
            Binding binding = new NetNamedPipeBinding();
            var address = "net.pipe://localhost/" + Guid.NewGuid();
            var instance = new MyService(3);
            using (var host =
                new ServiceHost(instance, new Uri(address)))
            {
                host.AddServiceEndpoint(typeof(IMyCounter), binding, "");

                host.Open();

                var proxy =
                    ChannelFactory<IMyCounter>.CreateChannel(
                        binding,
                        new EndpointAddress(address));
                Assert.AreEqual(3, instance.Count);
                Assert.AreEqual(4, proxy.IncrementCounterReleaseNone());
                Assert.AreEqual(4, instance.Count);
                ((ICommunicationObject)proxy).Close();
            }
        }

        [TestMethod]
        public void ReleaseInstanceModeBeforeTest()
        {
            Binding binding = new NetNamedPipeBinding();
            var address = "net.pipe://localhost/" + Guid.NewGuid();
            var instance = new MyService(3);
            using (var host =
                new ServiceHost(instance, new Uri(address)))
            {
                host.AddServiceEndpoint(typeof(IMyCounter), binding, "");

                host.Open();

                var proxy =
                    ChannelFactory<IMyCounter>.CreateChannel(
                        binding,
                        new EndpointAddress(address));
                Assert.AreEqual(3, instance.Count);
                Assert.AreEqual(1, proxy.IncrementCounterReleaseBefore());
                Assert.AreEqual(1, instance.Count);
                ((ICommunicationObject)proxy).Close();
            }
        }
    }
}