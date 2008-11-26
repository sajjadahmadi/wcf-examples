using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using System.ServiceModel.Dispatcher;
using System.Diagnostics;

namespace CodeRunner.ServiceModel.ThreadAffinity
{
    class ThreadAffinityHelper
    {
        static Dictionary<Type, AffinitySynchronizer> m_Contexts = new Dictionary<Type, AffinitySynchronizer>();

        [MethodImpl(MethodImplOptions.Synchronized)]
        internal static void ApplyDispatchBehavior(Type type, string threadName, DispatchRuntime dispatch)
        {
            Debug.Assert(dispatch.SynchronizationContext == null);

            if (m_Contexts.ContainsKey(type) == false)
            {
                m_Contexts[type] = new AffinitySynchronizer(threadName);
            }
            dispatch.SynchronizationContext = m_Contexts[type];
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void CloseThread(Type type)
        {
            if (m_Contexts.ContainsKey(type))
            {
                m_Contexts[type].Dispose();
                m_Contexts.Remove(type);
            }
        }
    }
}
