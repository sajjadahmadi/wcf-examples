using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using CodeRunner.Service;

namespace CodeRunner.Service
{
    [ServiceContract]
    internal interface ICalculator
    {
        [OperationContract]
        int Add(int number1, int number2);
    }

    internal class Calculator : ICalculator
    {
        #region ICalculator Members

        public int Add(int number1, int number2)
        {
            return number1 + number2;
        }

        #endregion
    }
}

namespace CodeRunner.Client
{
    [ServiceContract]
    internal interface ICalculator
    {
        [OperationContract]
        int Add(int number1, int number2);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginAdd(int number1, int number2, AsyncCallback callback, object asyncState);

        int EndAdd(IAsyncResult result);
    }

    internal class CalculatorClient : ClientBase<ICalculator>, ICalculator
    {
        public CalculatorClient(Binding binding, string address)
            : base(binding, new EndpointAddress(address))
        {
        }

        #region ICalculator Members

        public int Add(int number1, int number2)
        {
            return Channel.Add(number1, number2);
        }

        public IAsyncResult BeginAdd(int number1, int number2, AsyncCallback callback, object asyncState)
        {
            return Channel.BeginAdd(number1, number2, callback, asyncState);
        }

        public int EndAdd(IAsyncResult result)
        {
            return Channel.EndAdd(result);
        }

        #endregion
    }


    internal class Program
    {
        private static void Main(string[] args)
        {
            ServiceHost host;
            var address = "net.pipe://localhost/" + Guid.NewGuid();
            using (host = new ServiceHost(typeof(Calculator)))
            {
                host.AddServiceEndpoint(typeof(Service.ICalculator), new NetNamedPipeBinding(), address);
                host.Open();

                var proxy = new CalculatorClient(new NetNamedPipeBinding(), address);

                var call1 = proxy.BeginAdd(2, 3, null, null);
                var call2 = proxy.BeginAdd(4, 5, null, null);

                /* Do some work */

                var sum = proxy.EndAdd(call1);
                Debug.Assert(call1.IsCompleted);
                Debug.Assert(sum == 5);
                Console.WriteLine("Call 1: {0}", sum);

                sum = proxy.EndAdd(call2); // This may block
                Debug.Assert(call2.IsCompleted);
                Debug.Assert(sum == 9);
                Console.WriteLine("Call 1: {0}", sum);

                proxy.Close();
            }
        }
    }
}