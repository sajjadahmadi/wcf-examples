using System;
using System.ServiceModel;

namespace Contract_Inheritance {

    [ServiceContract]
    interface ISimpleCalculator {

        [OperationContract]
        Int32 Add(Int32 arg1, Int32 arg2);
    }

    [ServiceContract]
    interface IScientificCalculator : ISimpleCalculator {

        [OperationContract]
        Int32 Multiply(Int32 arg1, Int32 arg2);
    }

    class MyCalculator : IScientificCalculator {

        public int Multiply(int arg1, int arg2) {
            return arg1 * arg2;
        }

        public int Add(int arg1, int arg2) {
            return arg1 + arg2;
        }
    }
}
