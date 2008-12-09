using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace CodeRunner.Client
{
    [ServiceContract]
    public interface ICounter
    {
        [OperationContract]
        int Increment(int number);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginIncrement(int number, AsyncCallback callback, object asyncState);

        int EndIncrement(IAsyncResult result);
    }


    class CounterClient : ClientBase<ICounter>, ICounter
    {
        public CounterClient(Binding binding, string address)
            : base(binding, new EndpointAddress(address))
        { }

        public int Increment(int number)
        { return Channel.Increment(number); }

        public IAsyncResult BeginIncrement(int number1, AsyncCallback callback, object asyncState)
        { return Channel.BeginIncrement(number1, callback, asyncState); }

        public int EndIncrement(IAsyncResult result)
        { return Channel.EndIncrement(result); }
    }
}