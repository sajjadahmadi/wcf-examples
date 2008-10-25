using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ServiceModel.Examples
{
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
        [OperationContract(IsOneWay=true)]
        void OnCallback();
    }


    [ServiceBehavior(InstanceContextMode=InstanceContextMode.PerCall)]
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

    class ClientSideCallback : IMyContractCallback
    {
        public void OnCallback()
        {
            throw new NotImplementedException();
        }
    }

    [TestClass]
    public class CallbackOperations
    {
        [TestMethod]
        public void CallbackOperation()
        {
            IMyContractCallback callback = new ClientSideCallback();
            InstanceContext context = new InstanceContext(callback);
        }
    }

}
