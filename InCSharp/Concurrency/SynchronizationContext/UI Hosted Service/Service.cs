using System.ServiceModel;

namespace CodeRunner
{
    [ServiceContract]
    interface ICounter
    {
        [OperationContract]
        void Increment();
        [OperationContract]
        void Decrement();
        [OperationContract]
        int GetCount();
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    class CounterService : ICounter
    {
        public void Increment()
        {
            HostForm.Current.Counter++;
        }

        public void Decrement()
        {
            HostForm.Current.Counter--;
        }

        public int GetCount()
        {
            return HostForm.Current.Counter;
        }
    }
}
