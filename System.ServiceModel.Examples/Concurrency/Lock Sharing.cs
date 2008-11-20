
namespace System.ServiceModel.Examples
{
    static class Counter
    {
        public static void Increment()
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
            // Lock on the service type
            lock (typeof(MyService))
            {
                Counter.Increment(); 
            }
        }
    }
}
