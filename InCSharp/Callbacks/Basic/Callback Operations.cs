﻿using System.Collections.Generic;
using System.ServiceModel.Channels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ServiceModel.Examples
{
    #region Contracts
    [ServiceContract]
    interface IConnectionManagement
    {
        [OperationContract]
        void Connect();
        [OperationContract]
        void Disconnect();
    }

    [ServiceContract(CallbackContract = typeof(IMyContractCallback))]
    interface IMyContract : IConnectionManagement
    {
        // In addition to the callback operation, Transcender suggests 
        // making these one-way operations, also.
        // However, Juval (pg 172) recommends against it.  
        [OperationContract]
        void DoSomething();
    }

    interface IMyContractCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnCallback();
    }
    #endregion

    #region Service Side

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    class MyService : IMyContract
    {
        static List<IMyContractCallback> callbacks = new List<IMyContractCallback>();

        public void DoSomething()
        {
            CallClients();
        }

        public static void CallClients()
        {
            callbacks.ForEach(c => c.OnCallback());
        }

        #region IConnectionMangement Members

        public void Connect()
        {
            IMyContractCallback callback = OperationContext.Current.
                GetCallbackChannel<IMyContractCallback>();
            if (!callbacks.Contains(callback))
            {
                callbacks.Add(callback);
            }
        }

        public void Disconnect()
        {
            IMyContractCallback callback = OperationContext.Current.
                GetCallbackChannel<IMyContractCallback>();
            if (callbacks.Contains(callback))
            {
                callbacks.Remove(callback);
            }
        }

        #endregion
    }

    #endregion

    #region Client Side
    partial class MyContractClient : DuplexClientBase<IMyContract, IMyContractCallback>, IMyContract, IMyContractCallback
    {
        public MyContractClient(InstanceContext<IMyContractCallback> callback) : base(callback) { }
        public MyContractClient(InstanceContext<IMyContractCallback> callback, string endpointName) : base(callback, endpointName) { }
        public MyContractClient(InstanceContext<IMyContractCallback> callback, Binding binding, EndpointAddress remoteAddress) :
            base(callback, binding, remoteAddress) { }
        public MyContractClient(IMyContractCallback callback, Binding binding, EndpointAddress remoteAddress) :
            base(callback, binding, remoteAddress) { }

        #region IMyContract Members
        public void DoSomething()
        { Channel.DoSomething(); }
        #endregion

        #region IConnectionMangement Members
        public void Connect()
        {
            Channel.Connect();
            InnerDuplexChannel.CallbackInstance = new InstanceContext(this);
        }
        public void Disconnect()
        {
            Channel.Disconnect();
            InnerDuplexChannel.CallbackInstance = null;
        }
        #endregion

        #region IMyContractCallback Members
        public bool CallbackOccured = false;
        public void OnCallback()
        { CallbackOccured = true; }
        #endregion
    }

    class ClientSideCallback : IMyContractCallback
    {
        public bool CallbackOccured = false;
        public void OnCallback()
        { CallbackOccured = true; }
    }
    #endregion

    [TestClass]
    public class CallbackOperations
    {
        [TestMethod]
        public void DuplexClientBase()
        {
            string address = "net.pipe://localhost/" + Guid.NewGuid().ToString();
            using (ServiceHost<MyService> host = new ServiceHost<MyService>())
            {
                host.AddServiceEndpoint<IMyContract>(new NetNamedPipeBinding(), address);
                host.Open();

                IMyContractCallback callback = new ClientSideCallback();
                MyContractClient proxy = new MyContractClient(callback, new NetNamedPipeBinding(), new EndpointAddress(address));

                proxy.Connect();     // Register callbacks
                proxy.DoSomething();
                proxy.Disconnect();  // Unregister callbacks

                Assert.IsTrue(proxy.CallbackOccured);
            }
        }

        [TestMethod]
        public void DuplexChannelFactoryTest()
        {
            string address = "net.pipe://localhost/" + Guid.NewGuid().ToString();
            using (ServiceHost<MyService> host = new ServiceHost<MyService>())
            {
                host.AddServiceEndpoint<IMyContract>(new NetNamedPipeBinding(), address);
                host.Open();

                ClientSideCallback callback = new ClientSideCallback();
                IMyContract channel = DuplexChannelFactory<IMyContract, IMyContractCallback>
                    .CreateChannel(callback, new NetNamedPipeBinding(), new EndpointAddress(address));
                channel.Connect();
                channel.DoSomething();
                channel.Disconnect();
                Assert.IsTrue(((ClientSideCallback)callback).CallbackOccured);
            }
        }

        [TestMethod]
        public void ServiceHostDuplexChannelFactoryTest()
        {
            string address = "net.pipe://localhost/" + Guid.NewGuid().ToString();
            using (ServiceHost<MyService> host = new ServiceHost<MyService>())
            {
                host.AddServiceEndpoint<IMyContract>(new NetNamedPipeBinding(), address);
                host.OpenTimeout = new TimeSpan(0, 0, 30);
                host.Open();

                ClientSideCallback callback = new ClientSideCallback();
                IMyContract channel = host
                    .CreateChannel<IMyContract, IMyContractCallback>(callback, new NetNamedPipeBinding(), address);

                channel.Connect();
                channel.DoSomething();
                channel.Disconnect();
                Assert.IsTrue(((ClientSideCallback)callback).CallbackOccured);
            }
        }
    }

}
