using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Diagnostics;
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

                IAsyncResult result1 = proxy.BeginAdd(2, 3, null, null);
                IAsyncResult result2 = proxy.BeginAdd(4, 5, null, null);

                /* Do some work */

                int sum;

                result1.AsyncWaitHandle.WaitOne(10);  // This will wait up to 10ms for result1 to complete
                WaitHandle[] handleArray = { result1.AsyncWaitHandle, result2.AsyncWaitHandle };
                WaitHandle.WaitAll(handleArray, 10);  // This will wait up to 10ms for ALL to complete
                WaitHandle.WaitAny(handleArray, 10);  // This will wait up to 10ms for ANY to complete

                sum = proxy.EndAdd(result1);  // This will block only if wait above did not complete
                Debug.Assert(result1.IsCompleted == true);  
                Debug.Assert(sum == 5);

                sum = proxy.EndAdd(result2);  // This will block only if wait above did not complete
                Debug.Assert(result2.IsCompleted == true);
                Debug.Assert(sum == 9);

                proxy.Close();
            }
        }
    }
}

