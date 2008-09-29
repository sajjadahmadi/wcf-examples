using System;
using System.ServiceModel;

namespace WithOverloadedOperations {

    [ServiceContract]
    interface ICalculator {
        
        [OperationContract(Name = "AddInt")]
        Int32 Add(Int32 arg1, Int32 arg2);

        [OperationContract(Name = "AddDouble")]
        double Add(double arg1, double arg2);
    }

    class Calculator : ICalculator {

        public int Add(int arg1, int arg2) {
            return arg1 + arg2;
        }

        public double Add(double arg1, double arg2) {
            return arg1 + arg2;
        }
    }
}
