using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Transactions;
using CodeRunner.Transactions;
using System.Threading;

namespace Transactions.Tests
{
    [TestClass]
    public class TransactionalLock_Tests
    {
        [TestMethod]
        public void LockAndAbort()
        {
            TransactionalLock _lock;
            _lock = new TransactionalLock();
            using (TransactionScope scope = new TransactionScope())
            {
                Assert.IsTrue(Transaction.Current.TransactionInformation.Status == TransactionStatus.Active);
                _lock.Lock();
                Transaction.Current.Rollback();
                Assert.IsTrue(Transaction.Current.TransactionInformation.Status == TransactionStatus.Aborted);
            }
            Assert.IsNull(Transaction.Current);
            Assert.IsTrue(_lock.IsLocked, "Expected to still be locked.");
        }

        [TestMethod]
        public void LockAndComplete()
        {
            TransactionalLock _lock = new TransactionalLock();
            using (TransactionScope scope = new TransactionScope())
            {
                _lock.Lock();
                scope.Complete();
            }
            Assert.IsTrue(_lock.IsLocked, "Expected to still be locked.");
        }

        [TestMethod]
        public void LockAndUnlock()
        {
            TransactionalLock _lock;
            _lock = new TransactionalLock();
            using (TransactionScope scope = new TransactionScope())
            {
                _lock.Lock();
                _lock.Unlock();
            }
            Assert.IsFalse(_lock.IsLocked, "Expected to be unlocked.");
        }

        [TestMethod]
        public void MultipleLocksWithQueueAndTimeout()
        {
            TransactionalLock _lock = new TransactionalLock();

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew, new TimeSpan(0, 0, 1)))
            {
                _lock.Lock();
                Assert.IsTrue(_lock.IsLocked);
                Assert.AreEqual(0, _lock.PendingTransactionCount);
                Assert.Inconclusive("TODO: Complete this test!");
            }
        }

        [TestMethod]
        public void LockInsideSuppress()
        {
            TransactionalLock _lock;
            _lock = new TransactionalLock();
            using (TransactionScope scope = new TransactionScope())
            {
                _lock.Lock();
                using (TransactionScope scope2 = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    _lock.Lock();
                }
            }
        }

        [TestMethod]
        public void LockInsideSameTransaction()
        {
            TransactionalLock _lock;
            _lock = new TransactionalLock();
            using (TransactionScope scope = new TransactionScope())
            {
                _lock.Lock();
                using (TransactionScope scope2 = new TransactionScope())
                {
                    _lock.Lock();
                }
            }
        }

        [TestMethod]
        [Timeout(1000)]
        [Ignore]
        public void LockInsideNewTransaction_Deadlock()
        {
            TransactionalLock _lock;
            _lock = new TransactionalLock();
            using (TransactionScope scope = new TransactionScope())
            {
                _lock.Lock();
                using (TransactionScope scope2 = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    _lock.Lock();
                }
            }
        }
    }
}
