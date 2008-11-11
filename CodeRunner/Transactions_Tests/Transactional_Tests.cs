using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeRunner.Transactions;
using System.Transactions;

namespace Transactions.Tests
{
    [TestClass]
    public class Transactional_Tests
    {
        [TestMethod]
        public void AbortTransactionalValueType()
        {
            Transactional<int> tint = new Transactional<int>(5);
            using (TransactionScope scope = new TransactionScope())
            {
                tint.Value = 10;
                Assert.AreEqual(10, tint);
            }
            Assert.AreEqual(5, tint);
        }

        [TestMethod]
        public void CommitTransactionalValueType()
        {
            Transactional<int> tint = new Transactional<int>(5);
            using (TransactionScope scope = new TransactionScope())
            {
                tint.Value = 10;
                Assert.AreEqual(10, tint);
                scope.Complete();
            }
            Assert.AreEqual(10, tint);
        }


        [TestMethod]
        public void AbortTransactionalReferenceType()
        {
            Transactional<string> tstring = new Transactional<string>("5");
            using (TransactionScope scope = new TransactionScope())
            {
                tstring.Value = "10";
                Assert.AreEqual("10", tstring);
            }
            Assert.AreEqual("5", tstring);
        }

        [TestMethod]
        public void CommitTransactionalReferenceType()
        {
            Transactional<string> tstring = new Transactional<string>("5");
            using (TransactionScope scope = new TransactionScope())
            {
                tstring.Value = "10";
                Assert.AreEqual("10", tstring);
                scope.Complete();
            }
            Assert.AreEqual("10", tstring);
        }
    }
}
