using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Transactions;

namespace CodeRunner.Transactions
{
    public class Transactional<T> : ISinglePhaseNotification
    {
        T _value;
        T _tempValue;
        Transaction _currentTransaction;
        TransactionalLock _lock;

        public Transactional(T value)
        {
            _lock = new TransactionalLock();
            _value = value;
        }
        public Transactional()
            : this(default(T)) { }
        static Transactional()
        {
            ResourceManager.ConstrainType(typeof(T));
        }


        void ISinglePhaseNotification.SinglePhaseCommit(SinglePhaseEnlistment singlePhaseEnlistment)
        {
            Commit();
            singlePhaseEnlistment.Committed();
        }
        void Commit()
        {
            IDisposable disposable = _value as IDisposable;
            if (disposable != null) { disposable.Dispose(); }
            _value = _tempValue;
            _currentTransaction = null;
            _tempValue = default(T);
            _lock.Unlock();
        }

        #region IEnlistmentNotification
        void IEnlistmentNotification.Commit(Enlistment enlistment)
        {
            Commit();
            enlistment.Done();
        }
        void IEnlistmentNotification.InDoubt(Enlistment enlistment)
        {
            _lock.Unlock();
            enlistment.Done();
        }
        void IEnlistmentNotification.Prepare(PreparingEnlistment preparingEnlistment)
        {
            preparingEnlistment.Prepared();
        }
        void IEnlistmentNotification.Rollback(Enlistment enlistment)
        {
            _currentTransaction = null;

            IDisposable disposable = _tempValue as IDisposable;
            if (disposable != null)
            { disposable.Dispose(); }

            _tempValue = default(T);
            _lock.Unlock();
            enlistment.Done();
        }
        void Enlist(T t)
        {
            Debug.Assert(_currentTransaction == null);
            _currentTransaction = Transaction.Current;
            Debug.Assert(_currentTransaction.TransactionInformation.Status == TransactionStatus.Active);
            _currentTransaction.EnlistVolatile(this, EnlistmentOptions.None);
            _tempValue = ResourceManager.Clone(t);
        }

        #endregion

        void SetValue(T v)
        {
            _lock.Lock();
            if (_currentTransaction == null)
            {
                if (Transaction.Current == null)
                {
                    // No ambient transaction, so just set the value
                    _value = v;
                    return;
                }
                else
                {
                    Enlist(v);
                    return;
                }
            }
            else
            {
                // Must have acquired the lock
                Debug.Assert(_currentTransaction == Transaction.Current, "Invalid state.");
                _tempValue = v;
            }
        }
        T GetValue()
        {
            _lock.Lock();
            if (_currentTransaction == null)
            {
                if (Transaction.Current == null)
                {
                    return _value;
                }
                else
                {
                    Enlist(_value);
                }
            }
            // Must have acquired the lock
            Debug.Assert(_currentTransaction == Transaction.Current, "Invalid state.");
            return _tempValue;
        }
        public T Value
        {
            get { return GetValue(); }
            set { SetValue(value); }
        }

        #region Operators
        public static implicit operator T(Transactional<T> transactional)
        { return transactional.Value; }
        public static bool operator ==(Transactional<T> t1, Transactional<T> t2)
        {
            T v1 = default(T);
            T v2 = default(T);
            if (!object.ReferenceEquals(t1, null)) v1 = t1.Value;
            if (!object.ReferenceEquals(t2, null)) v2 = t2.Value;
            return EqualityComparer<T>.Default.Equals(v1, v2);
        }
        public static bool operator !=(Transactional<T> t1, Transactional<T> t2)
        { return !(t1 == t2); }
        public override int GetHashCode()
        { return Value.GetHashCode(); }
        public override bool Equals(object obj)
        { return Value.Equals(obj); }
        #endregion
    }
}
