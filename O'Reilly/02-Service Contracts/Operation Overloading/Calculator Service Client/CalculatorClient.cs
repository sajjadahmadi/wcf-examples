using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.ServiceModel.Examples
{
    [ServiceContract]
    public interface ICalculator
    {
        [OperationContract(Name = "AddInt")]
        int Add(int arg1, int arg2);

        [OperationContract(Name = "AddDouble")]
        int Add(double arg1, double arg2);
    }

    class CalculatorClient : ClientBase<ICalculator>, ICalculator
    {

        #region ICalculator Members

        public int Add(int operand1, int operand2)
        {
            return Channel.Add(operand1,operand2);
        }

        public int Add(double operand1, double operand2)
        {
            return Channel.Add(operand1, operand2);
        }

        #endregion
    }
}
