using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Permissions;
using System.Threading;

namespace CodeRunner.ServiceModel.ThreadAffinity
{
    [SecurityPermission(SecurityAction.Demand, ControlThread = true)]
    class AffinitySynchronizer : SynchronizationContext, IDisposable
    {
        WorkerThread m_WorkerThread;

        public AffinitySynchronizer()
            : this("AffinitySynchronizer Worker Thread")
        { }
        public AffinitySynchronizer(string threadName)
        {
            m_WorkerThread = new WorkerThread(threadName, this);
        }

        public override SynchronizationContext CreateCopy()
        {
            return this;  // TODO: Why?
        }
        public override void Post(SendOrPostCallback method, object state)
        {
            WorkItem workItem = new WorkItem(method, state);
            m_WorkerThread.QueueWorkItem(workItem);
        }
        public override void Send(SendOrPostCallback method, object state)
        {
            // If this is already the current context, 
            // must invoke the method directly to avoid a deadlock
            if (SynchronizationContext.Current == this)
            {
                method(state);
                return;
            }
            WorkItem workItem = new WorkItem(method, state);
            m_WorkerThread.QueueWorkItem(workItem);
            workItem.AsyncWaitHandle.WaitOne();
        }

        public void Dispose()
        {
            m_WorkerThread.Kill();
        }
    }

    internal class WorkerThread
    {
        class WorkItemQueue : Queue<WorkItem>
        {
            public bool HasItems
            {
                get { lock (this) { return (this.Count > 0); } }
            }
            public new WorkItem Dequeue()
            {
                if (HasItems)
                {
                    lock (this)
                    { return base.Dequeue(); }
                }
                return null;
            }
            public new void Enqueue(WorkItem item)
            {
                lock (this)
                { base.Enqueue(item); }
            }

        }

        SynchronizationContext m_Context;

        AutoResetEvent m_ItemAdded;
        WorkItemQueue m_WorkItems;

        internal WorkerThread(string name, SynchronizationContext context)
        {
            m_Context = context;

            m_EndLoop = false;
            m_ItemAdded = new AutoResetEvent(false);
            m_WorkItems = new WorkItemQueue();

            m_ThreadObj = new Thread(Run);
            m_ThreadObj.IsBackground = true;
            m_ThreadObj.Name = name;
            m_ThreadObj.Start();
        }

        internal void QueueWorkItem(WorkItem workItem)
        {
            m_WorkItems.Enqueue(workItem);
            m_ItemAdded.Set();
        }

        // TODO: Why are these exposed?
        public Thread m_ThreadObj;
        public int ManagedThreadId
        {
            get { return m_ThreadObj.ManagedThreadId; }
        }

        bool m_EndLoop;
        bool EndLoop
        {
            get
            { lock (this) { return m_EndLoop; } }
            set
            { lock (this) { m_EndLoop = value; } }
        }

        void Run()
        {
            Debug.Assert(SynchronizationContext.Current == null);
            SynchronizationContext.SetSynchronizationContext(m_Context);

            while (EndLoop == false)
            {
                while (m_WorkItems.HasItems)
                {
                    if (EndLoop == true) return;

                    WorkItem workItem = m_WorkItems.Dequeue();
                    workItem.DoWork();
                }
                m_ItemAdded.WaitOne();
            }
        }

        // TODO: Couldn't this be internal?
        public void Kill()
        {
            Debug.Assert(m_ThreadObj != null);
            if (m_ThreadObj.IsAlive == false) return;
            EndLoop = true;
            m_ItemAdded.Set();

            // Wait for thread to stop
            m_ThreadObj.Join();

            // TODO: Wouldn't the Set above throw an exception if this was true?
            if (m_ItemAdded != null) m_ItemAdded.Close();
        }
    }

    //TODO: Do we need [Serializable]?
    internal class WorkItem
    {
        object m_state;
        SendOrPostCallback m_method;
        ManualResetEvent m_AsyncWaitHandle;

        internal WorkItem(SendOrPostCallback method, object state)
        {
            m_method = method;
            m_state = state;
            m_AsyncWaitHandle = new ManualResetEvent(false);
        }

        public WaitHandle AsyncWaitHandle
        {
            get { return m_AsyncWaitHandle; }
        }

        // This method is called on the worker thread to execute the method
        internal void DoWork()
        {
            m_method(m_state);
            m_AsyncWaitHandle.Set();
        }
    }
}
