using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using CodeRunner.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ServiceModel.Examples
{
    [TestClass]
    public class IsolationLevels
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

        [TestMethod]
        public void SerializableTest()
        {
            var options = new TransactionOptions() { IsolationLevel = IsolationLevel.Serializable };
            using (var scope1 = new TransactionScope(TransactionScopeOption.Required, options))
            {
                var s = new Transactional<string>("hi");
                s.Value = "new value";
                using (new TransactionScope(TransactionScopeOption.Required, options))
                {
                    Assert.AreEqual("hi", s.Value);
                }
            }
        }
    }
}
