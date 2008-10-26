using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.ServiceModel.Channels;

namespace System.ServiceModel.Examples
{
    #region Contracts
    [ServiceContract]
    interface IConnectionMangement
    {
        [OperationContract]
        void Connect();
        [OperationContract]
        void Disconnect();
    }

    [ServiceContract(CallbackContract = typeof(IMyContractCallback))]
    interface IMyContract : IConnectionMangement
    {
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
            string address = "net.pipe://localhost/";
            using (ServiceHost<MyService> host = new ServiceHost<MyService>())
            {
                host.AddServiceEndpoint<IMyContract>(new NetNamedPipeBinding(), address);
                host.Open();

                IMyContractCallback callback = new ClientSideCallback();
                MyContractClient proxy = new MyContractClient(callback, new NetNamedPipeBinding(), new EndpointAddress(address));

                proxy.Connect();
                proxy.DoSomething();
                proxy.Disconnect();

                Assert.IsTrue(proxy.CallbackOccured);
            }
        }

        [TestMethod]
        public void DuplexChannelFactoryTest()
        {
            string address = "net.pipe://localhost/";
            using (ServiceHost<MyService> host = new ServiceHost<MyService>())
            {
                host.AddServiceEndpoint<IMyContract>(new NetNamedPipeBinding(), address);
                host.Open();

                IMyContractCallback callback = new ClientSideCallback();
                IMyContract channel = DuplexChannelFactory<IMyContract, IMyContractCallback>
                    .CreateChannel(callback, new NetNamedPipeBinding(), new EndpointAddress(address));
                channel.Connect();
                channel.DoSomething();
                channel.Disconnect();
                Assert.IsTrue(((ClientSideCallback)callback).CallbackOccured);
            }
        }
    }

}
