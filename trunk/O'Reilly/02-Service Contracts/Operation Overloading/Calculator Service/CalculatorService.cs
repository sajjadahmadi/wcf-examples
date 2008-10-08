using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace System.ServiceModel.Examples
{
    [ServiceContract]
    public interface ICalculator
    {
        [OperationContract(Name="AddInt")]
        int Add(int operand1, int operand2);

        [OperationContract(Name="AddDouble")]
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
