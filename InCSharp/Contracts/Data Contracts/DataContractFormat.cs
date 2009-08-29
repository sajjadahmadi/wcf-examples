using System;
using System.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WcfExamples
{
    [TestClass]
    public class DataContractFormat
    {
        [TestMethod]
        public void TestMethod1()
        {
            // TODO
        }

        #region Nested type: ICalculator

        [ServiceContract, DataContractFormat(Style = OperationFormatStyle.Rpc)]
        private interface ICalculator
        {
            [OperationContract, DataContractFormat(Style = OperationFormatStyle.Rpc)]
            double Add(double a, double b);

            [OperationContract, DataContractFormat(Style = OperationFormatStyle.Document)]
            double Subtract(double a, double b);
        }

        #endregion
    }
}