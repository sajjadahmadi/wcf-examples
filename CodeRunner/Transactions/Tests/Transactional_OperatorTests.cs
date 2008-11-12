using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeRunner.Transactions;

namespace Transactions.Tests
{
    [TestClass]
    public class Transactional_OperatorTests
    {
        [TestMethod]
        public void ImplicitOperator()
        {
            Transactional<int> tint = new Transactional<int>(6);
            int expected = 6;
            Assert.AreEqual(expected, tint);
        }

        [TestMethod]
        public void Equality_BothNull()
        {
            Transactional<string> t1 = null;
            Transactional<string> t2 = null;
            Assert.AreEqual(t1, t2);
            Assert.IsTrue(t1 == t2);
            Assert.IsTrue(t1 == null);
        }

        [TestMethod]
        public void Equality_SameValue()
        {
            Transactional<string> t1 = new Transactional<string>("text");
            Transactional<string> t2 = new Transactional<string>("text");
            Assert.IsTrue(t1 == t2);
            Assert.IsTrue(t1 == "text");
        }

        [TestMethod]
        public void Inquality_OneNull()
        {
            Transactional<string> t1 = new Transactional<string>("string");
            Transactional<string> t2 = null;
            Assert.AreNotEqual(t1, t2);
            Assert.IsTrue(t1 != t2);
            Assert.IsTrue(t1 != null);
        }

        [TestMethod]
        public void Inquality_DifferentValue()
        {
            Transactional<string> t1 = new Transactional<string>("text");
            Transactional<string> t2 = new Transactional<string>("string");
            Assert.AreNotEqual(t1, t2);
            Assert.IsTrue(t1 != t2);
        }
    }
}
