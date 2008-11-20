using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel;

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
}
