using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ServiceModel.Test
{
    [TestClass]
    public class InProcFactoryTests
    {
        #region Service
        [ServiceContract]
        interface ITestContract
        {
            [OperationContract]
            string MyOperation();
        }

        class TestService : ITestContract
        {
            public string MyOperation()
            {
                return "MyResult";
            }
        }
        #endregion

        [TestMethod]
        public void CreateChannelProxy()
        {
            ITestContract proxy1 = InProcFactory.CreateChannel<TestService, ITestContract>();
            Assert.AreEqual<string>("MyResult", proxy1.MyOperation());
            InProcFactory.CloseChannel<ITestContract>(proxy1);
        }

        [TestMethod]
        public void CreateMultipleChannelProxies()
        {
            ITestContract proxy1 = InProcFactory.CreateChannel<TestService, ITestContract>();
            ITestContract proxy2 = InProcFactory.CreateChannel<TestService, ITestContract>();
            Assert.AreNotSame(proxy1, proxy2);
            Assert.AreEqual<string>("MyResult", proxy1.MyOperation());
            Assert.AreEqual<string>("MyResult", proxy2.MyOperation());
        }
    }
}
