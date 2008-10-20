using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace System.ServiceModel.Examples
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
        public int Add(int operand1, int operand2)
        {
            return Channel.Add(operand1, operand2);
        }

        public double Add(double operand1, double operand2)
        {
            return Channel.Add(operand1, operand2);
        }
    }
}
