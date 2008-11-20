using System.Threading;

namespace System.ServiceModel.Examples
{
    class Counter
    {
        static int current = 0;

        public static int Increment()
        {
            int result = 0;
            SendOrPostCallback doWork =
                delegate
                {
                    result = IncrementInternal();
                };
            return result;
        }

        SynchronizationContext MySynchronizationContext
        { get; }

        static int IncrementInternal()
        {
            return ++current;
        }
    }

    [ServiceContract]
    interface IMyContract
    {
        int NextValue();
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    class MyService : IMyContract
    {
        public int NextValue()
        {
            throw new NotImplementedException();
        }
    }
}