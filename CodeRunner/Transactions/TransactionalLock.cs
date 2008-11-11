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
        TransactionQueue<KeyValuePair<Transaction, ManualResetEvent>> pendingTransactions
            = new TransactionQueue<KeyValuePair<Transaction, ManualResetEvent>>();
        class TransactionQueue<T> : LinkedList<T>
        {
            public void Enqueue(T item)
            {
                base.AddLast(item);
            }

            public T Dequeue()
            {
                T item = base.First.Value;
                base.RemoveFirst();
                return item;
            }
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
                    // Queue up the transaction and wait for the other one to complete
                    manualEvent = QueueTransaction(transaction);
                }
            }
            finally
            {
                Monitor.Exit(this);
            }
            
            manualEvent.WaitOne();  // Block the calling thread 
            lock (manualEvent) { manualEvent.Close(); }
        }

        ManualResetEvent QueueTransaction(Transaction transaction)
        {
            Debug.Assert(transaction.TransactionInformation.Status == TransactionStatus.Active);

            ManualResetEvent manualEvent = new ManualResetEvent(false);
            KeyValuePair<Transaction, ManualResetEvent> pair
                = new KeyValuePair<Transaction, ManualResetEvent>(transaction, manualEvent);
            pendingTransactions.Enqueue(pair);

            // Since the transaction could abort or time out while in the queued,
            // unblock it and remove it from the queue when it completes 
            //transaction.TransactionCompleted += delegate
            //{
            //    lock (this)
            //    {
            //        pendingTransactions.Remove(pair);
            //    }
            //    lock (manualEvent)
            //    {
            //        if (manualEvent.SafeWaitHandle.IsClosed == false)
            //        { manualEvent.Set(); }
            //    }
            //};

            return manualEvent;
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
                KeyValuePair<Transaction, ManualResetEvent> pair = pendingTransactions.Dequeue();
                Transaction transaction = pair.Key;
                ManualResetEvent manualEvent = pair.Value;

                Lock(transaction); 

                lock (manualEvent)
                {
                    if (manualEvent.SafeWaitHandle.IsClosed==false)
                    { manualEvent.Set(); }
                }
            }
        }

    }
}
