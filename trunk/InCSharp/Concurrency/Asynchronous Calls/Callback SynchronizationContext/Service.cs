using System.ServiceModel;

namespace CodeRunner.Service
{
    [ServiceContract]
    interface ICounter
    {
        [OperationContract]
        int Increment(int number);
    }
    class Counter : ICounter
    {
        public int Increment(int number)
        {
            return ++number;
        }
    }
}