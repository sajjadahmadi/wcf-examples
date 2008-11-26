using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel;
using CodeRunner.ServiceModel.ThreadAffinity;
using System.Threading;

namespace CodeRunner.ServiceModel.Test
{
    /// <summary>
    /// Summary description for ThreadAffinityBehaviorAttributeTests
    /// </summary>
    [TestClass]
    public class ThreadAffinityBehaviorAttributeTests
    {
        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [ServiceContract]
        interface IMyContract
        {
            [OperationContract]
            string GetThreadName();
        }

        [ServiceBehavior(InstanceContextMode= InstanceContextMode.PerCall)]
        [ThreadAffinityBehaviorAttribute(typeof(MyService), "Service Thread")]
        class MyService : IMyContract
        {
            public string GetThreadName()
            {
                return Thread.CurrentThread.Name;
            }
        }

        [TestMethod]
        public void ThreadAffinityBehaviorAttributeTest()
        {
            Assert.IsNull(SynchronizationContext.Current);
            IMyContract channel = InProcFactory.CreateChannel<MyService, IMyContract>();
            Assert.AreEqual("Service Thread", channel.GetThreadName());
            InProcFactory.CloseChannel<IMyContract>(channel);
        }
    }
}
