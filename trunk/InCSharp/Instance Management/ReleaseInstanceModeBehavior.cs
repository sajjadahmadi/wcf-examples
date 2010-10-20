using System.ServiceModel.Channels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ServiceModel.Examples
{
    [ServiceContract]
    interface IMyCounter
    {
        [OperationContract]
        void SetCount(int count);

        [OperationContract]
        int GetCount();

        [OperationContract]
        int IncrementWithoutRelease();

        [OperationContract]
        int ReleaseBeforeIncrement();

        [OperationContract]
        int ReleaseAfterIncrement();

        [OperationContract]
        int ReleaseBeforeAndAfterIncrement();
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, IncludeExceptionDetailInFaults = true)]
    internal class MyService : IMyCounter
    {
        public int Count;

        public void SetCount(int count)
        {
            Count = count;
        }

        public int GetCount()
        {
            return Count;
        }


        [OperationBehavior(ReleaseInstanceMode = ReleaseInstanceMode.None)]
        public int IncrementWithoutRelease()
        {
            return ++Count;
        }

        [OperationBehavior(ReleaseInstanceMode = ReleaseInstanceMode.BeforeCall)]
        public int ReleaseBeforeIncrement()
        {
            return ++Count;
        }

        [OperationBehavior(ReleaseInstanceMode = ReleaseInstanceMode.AfterCall)]
        public int ReleaseAfterIncrement()
        {
            return ++Count;
        }

        [OperationBehavior(ReleaseInstanceMode = ReleaseInstanceMode.BeforeAndAfterCall)]
        public int ReleaseBeforeAndAfterIncrement()
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
            using (var host =
                new ServiceHost(typeof(MyService), new Uri(address)))
            {
                host.AddServiceEndpoint(typeof(IMyCounter), binding, "");
                host.Open();

                var proxy = ChannelFactory<IMyCounter>.CreateChannel(binding, new EndpointAddress(address));
                proxy.SetCount(3);
                Assert.AreEqual(3, proxy.GetCount());
                Assert.AreEqual(4, proxy.IncrementWithoutRelease());
                Assert.AreEqual(4, proxy.GetCount());
                ((ICommunicationObject)proxy).Close();
            }
        }

        [TestMethod]
        public void ReleaseInstanceModeBeforeTest()
        {
            Binding binding = new NetNamedPipeBinding();
            var address = "net.pipe://localhost/" + Guid.NewGuid();
            using (var host =
                new ServiceHost(typeof(MyService), new Uri(address)))
            {
                host.AddServiceEndpoint(typeof(IMyCounter), binding, "");
                host.Open();

                var proxy = ChannelFactory<IMyCounter>.CreateChannel(binding, new EndpointAddress(address));
                proxy.SetCount(3);
                Assert.AreEqual(3, proxy.GetCount());
                Assert.AreEqual(1, proxy.ReleaseBeforeIncrement());
                Assert.AreEqual(1, proxy.GetCount());
                ((ICommunicationObject)proxy).Close();
            }
        }

        [TestMethod]
        public void ReleaseInstanceModeAfterTest()
        {
            Binding binding = new NetNamedPipeBinding();
            var address = "net.pipe://localhost/" + Guid.NewGuid();
            using (var host =
                new ServiceHost(typeof(MyService), new Uri(address)))
            {
                host.AddServiceEndpoint(typeof(IMyCounter), binding, "");
                host.Open();

                var proxy = ChannelFactory<IMyCounter>.CreateChannel(binding, new EndpointAddress(address));
                proxy.SetCount(3);
                Assert.AreEqual(3, proxy.GetCount());
                Assert.AreEqual(4, proxy.ReleaseAfterIncrement());
                Assert.AreEqual(0, proxy.GetCount());
                ((ICommunicationObject)proxy).Close();
            }
        }

        [TestMethod]
        public void ReleaseInstanceModeBeforeAndAfterTest()
        {
            Binding binding = new NetNamedPipeBinding();
            var address = "net.pipe://localhost/" + Guid.NewGuid();
            using (var host =
                new ServiceHost(typeof(MyService), new Uri(address)))
            {
                host.AddServiceEndpoint(typeof(IMyCounter), binding, "");
                host.Open();

                var proxy = ChannelFactory<IMyCounter>.CreateChannel(binding, new EndpointAddress(address));
                proxy.SetCount(3);
                Assert.AreEqual(3, proxy.GetCount());
                Assert.AreEqual(1, proxy.ReleaseBeforeAndAfterIncrement());
                Assert.AreEqual(0, proxy.GetCount());
                ((ICommunicationObject)proxy).Close();
            }
        }

    }
}