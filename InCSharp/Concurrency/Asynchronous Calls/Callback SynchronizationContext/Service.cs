using System.ServiceModel;
using System.Threading;

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
            Thread.Sleep(1000);
            return ++number;
        }
    }
}