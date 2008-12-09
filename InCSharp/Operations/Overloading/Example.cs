using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace CodeRunner.Service
{
    [ServiceContract]
    public interface ICalculator
    {
        [OperationContract(Name = "AddInt")]
        int Add(int operand1, int operand2);

        [OperationContract(Name = "AddDouble")]
        double Add(double operand1, double operand2);
    }

    public class Calculator : ICalculator
    {
        public int Add(int operand1, int operand2)
        {
            return operand1 + operand2;
        }

        public double Add(double operand1, double operand2)
        {
            return operand1 + operand2;
        }
    }
}

namespace CodeRunner.Client
{
    [ServiceContract(ConfigurationName = "ICalculator")]
    public interface ICalculator
    {
        [OperationContract(Name = "AddInt")]
        int Add(int operand1, int operand2);

        [OperationContract(Name = "AddDouble")]
        double Add(double operand1, double operand2);
    }

    class CalculatorClient : ClientBase<ICalculator>, ICalculator
    {
        static Uri address = new Uri("net.pipe://localhost/"+ Guid.NewGuid().ToString());

        public CalculatorClient()
            : base(new NetNamedPipeBinding(), new EndpointAddress(address))
        { }

        public int Add(int operand1, int operand2)
        {
            return Channel.Add(operand1, operand2);
        }

        public double Add(double operand1, double operand2)
        {
            return Channel.Add(operand1, operand2);
        }

        static void Main(string[] args)
        {
            ServiceHost host;
            using (host = new ServiceHost(typeof(Service.Calculator), address))
            {
                host.AddServiceEndpoint(typeof(Service.ICalculator), new NetNamedPipeBinding(), "");
                host.Open();

                double double1 = 10, double2 = 3;
                double doubleA;
                int int1 = 10, int2 = 3;
                int intA;

                CalculatorClient proxy = new CalculatorClient();

                doubleA = proxy.Add(double1, double2);
                Console.WriteLine("c2.Add(double1, double2)=c2.Add({0}, {1})={2}", double1, double2, doubleA);

                intA = proxy.Add(int1, int2);
                Console.WriteLine("c2.Add(int1, int2)=c2.Add({0}, {1})={2}", int1, int2, intA);

                proxy.Close();

                Console.ReadKey(true);
            }
        }
    }
}

