using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ServiceModel.Examples
{
    interface IServiceContractCallback
    {
        [OperationContract(IsOneWay=true)]
        void OnCallback();
    }

    [ServiceContract(CallbackContract = typeof(IServiceContractCallback))]
    interface IServiceContract
    {
        [OperationContract]
        void DoSomething();
    }

    [ServiceBehavior(ConcurrencyMode=ConcurrencyMode.Reentrant)]
    class MyService : IServiceContract
    {
        public void DoSomething()
        {
            throw new NotImplementedException();
        }
    }

    class ClientSideCallback : IServiceContractCallback
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
            IServiceContractCallback callback = new ClientSideCallback();
            InstanceContext context = new InstanceContext(callback);
        }
    }

}
