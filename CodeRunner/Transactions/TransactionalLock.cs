using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Threading;
using System.Diagnostics;

namespace CodeRunner.Transactions
{
    /// <summary>
    /// Provides exclusive transaction isolation (Serializable isolation level only)
    /// </summary>
    public class TransactionalLock
    {
        TransactionQueue pendingTransactions
            = new TransactionQueue();
        class TransactionQueue : LinkedList<QueueItem>
        {
            //LinkedList<QueueItem> queue;
            public ManualResetEvent Enqueue(Transaction transaction)
            {
                QueueItem item = new QueueItem(transaction, new ManualResetEvent(false));
                AddLast(item);
                return item.ManualEvent;
            }

            public QueueItem Dequeue()
            {
                QueueItem item = First.Value;
                RemoveFirst();
                return item;
            }

            public void Remove(Transaction transaction)
            {
                QueueItem item = this.SingleOrDefault<QueueItem>(i => i.Transaction == transaction);
                if (item == null) return;
                Remove(item);
                ManualResetEvent manualEvent = item.ManualEvent;
                lock (manualEvent)
                {
                    if (manualEvent.SafeWaitHandle.IsClosed == false)
                    { manualEvent.Set(); }
                }
            }
        }
        class QueueItem
        {
            public QueueItem(Transaction transaction, ManualResetEvent manualEvent)
            {
                this.Transaction = transaction;
                this.ManualEvent = manualEvent;
            }
            public Transaction Transaction { get; set; }
            public ManualResetEvent ManualEvent { get; set; }
        }


        Transaction owningTransaction;
        Transaction OwningTransaction
        {
            get
            {
                lock (this)
                { return owningTransaction; }
            }
            set
            {
                lock (this)
                { owningTransaction = value; }
            }
        }

        public bool IsLocked
        { get { return OwningTransaction != null; } }

        public int PendingTransactionCount
        { get { return pendingTransactions.Count; } }

        /// <summary>
        /// Acquires a lock for the exclusive use of a transaction.
        /// If another transaction owns the lock, it blocks the calling transaction
        /// and places it in a queue.  
        /// If the calling transaction already owns the lock, Lock() does nothing.
        /// </summary>
        public void Lock()
        {
            Lock(Transaction.Current);
        }
        void Lock(Transaction transaction)
        {
            Monitor.Enter(this);

            ManualResetEvent manualEvent;
            try
            {
                Debug.WriteLine(Thread.CurrentThread.Name);

                if (transaction == null)
                { return; }

                if (OwningTransaction == transaction)
                { return; }

                if (OwningTransaction == null)
                {
                    Debug.Assert(transaction.IsolationLevel == IsolationLevel.Serializable);
                    OwningTransaction = transaction;  // Acquire the lock
                    return;
                }
                else // Some other transaction owns the lock
                {
                    Debug.Assert(transaction.TransactionInformation.Status == TransactionStatus.Active);

                    // Queue up the transaction and wait for the other one to complete
                    manualEvent = pendingTransactions.Enqueue(transaction);

                    // Since the transaction could abort or time out while in the queued,
                    // unblock it and remove it from the queue when it completes 
                    transaction.TransactionCompleted += 
                        delegate
                        {
                            lock (this)
                            {
                                pendingTransactions.Remove(transaction);
                            }
                        };
                }
            }
            finally
            {
                Monitor.Exit(this);
            }

            manualEvent.WaitOne();  // Block the calling thread 
            lock (manualEvent) { manualEvent.Close(); }
        }


        /// <summary>
        /// Releases the transaction lock and allows the next pending transaction to aquire it.
        /// </summary>
        public void Unlock()
        {
            Debug.Assert(IsLocked);
            lock (this)
            {
                OwningTransaction = null;  // Release
                AquireNextLock();
            }
        }
        void AquireNextLock()
        {
            if (pendingTransactions.Count > 0)
            {
                QueueItem item = pendingTransactions.Dequeue();
                Transaction transaction = item.Transaction;
                ManualResetEvent manualEvent = item.ManualEvent;

                Lock(transaction);

                lock (manualEvent)
                {
                    if (manualEvent.SafeWaitHandle.IsClosed == false)
                    { manualEvent.Set(); }
                }
            }
        }

    }
}
