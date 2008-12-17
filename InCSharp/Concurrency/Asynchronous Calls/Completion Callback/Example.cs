using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;

namespace CodeRunner.Service
{
    [ServiceContract]
    interface ICalculator
    {
        [OperationContract]
        int Add(int number1, int number2);
    }
    class Calculator : ICalculator
    {
        public int Add(int number1, int number2)
        {
            return number1 + number2;
        }
    }
}

namespace CodeRunner.Client
{
    [ServiceContract]
    public interface ICalculator
    {
        [OperationContract]
        int Add(int number1, int number2);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginAdd(int number1, int number2, AsyncCallback callback, object asyncState);

        int EndAdd(IAsyncResult result);
    }


    class CalculatorClient : ClientBase<ICalculator>, ICalculator
    {
        public CalculatorClient(Binding binding, string address)
            : base(binding, new EndpointAddress(address))
        { }

        public int Add(int number1, int number2)
        { return Channel.Add(number1, number2); }

        public IAsyncResult BeginAdd(int number1, int number2, AsyncCallback callback, object asyncState)
        { return Channel.BeginAdd(number1, number2, callback, asyncState); }

        public int EndAdd(IAsyncResult result)
        { return Channel.EndAdd(result); }
    }

    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost<Service.Calculator> host;
            string address = "net.pipe://localhost/" + Guid.NewGuid().ToString();
            using (host = new ServiceHost<CodeRunner.Service.Calculator>())
            {
                host.AddServiceEndpoint<Service.ICalculator>(new NetNamedPipeBinding(), address);
                host.Open();

                CalculatorClient proxy = new CalculatorClient(new NetNamedPipeBinding(), address);

                ManualResetEvent completeEvent = new ManualResetEvent(false);
                int sum = 0;

                AsyncCallback completion = delegate(IAsyncResult result)
                {
                    Debug.Assert(result.IsCompleted == true);
                    sum = proxy.EndAdd(result);  // This will not block
                    completeEvent.Set();
                };

                proxy.BeginAdd(2, 3, completion, null);

                /* Do some work */

                completeEvent.WaitOne();
                Debug.Assert(sum == 5);

                proxy.Close();
            }
        }
    }
}

