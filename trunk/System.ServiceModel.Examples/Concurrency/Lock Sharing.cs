using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel;
using System.Threading;

namespace Concurrency
{
    static class MyResource
    {
        public static void DoWork()
        {
            // Lock on the service type
            lock (typeof(MyService))
            {
                // Access resource
            }
        }
    }

    [ServiceContract]
    interface IMyContract
    {
        [OperationContract]
        void MyMethod();
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    class MyService : IMyContract
    {
        public void MyMethod()
        {
            /// Lock on the service type
            lock (typeof(MyService))
            {
                MyResource.DoWork(); 
            }
        }
    }

    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            IMyContract proxy = InProcFactory.CreateChannel<MyService, IMyContract>();

            proxy.MyMethod();

            ((ICommunicationObject)proxy).Close();
        }
    }
}
