using System.ServiceModel;

namespace CodeRunner
{
    [ServiceContract(CallbackContract = typeof(ICounterCallback))]
    interface ICounter
    {
        [OperationContract]
        void Increment();
        [OperationContract]
        void Decrement();
        [OperationContract]
        int GetCount();
    }

    interface ICounterCallback
    {
        [OperationContract]
        void CountChanged(int value);
    }

    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerCall,
        ConcurrencyMode = ConcurrencyMode.Reentrant)]
    class CounterService : ICounter
    {
        public void Increment()
        {
            int value;
            value = ++HostForm.Current.Count;
            OnCountChanged(value);
        }

        public void Decrement()
        {
            int value;
            value = --HostForm.Current.Count;
            OnCountChanged(value);
        }

        void OnCountChanged(int value)
        {
            OperationContext context = OperationContext.Current;
            if (context == null) return;
            ICounterCallback callback = context.GetCallbackChannel<ICounterCallback>();
            if (callback == null) return;
            callback.CountChanged(value);
        }

        public int GetCount()
        {
            return HostForm.Current.Count;
        }
    }
}
