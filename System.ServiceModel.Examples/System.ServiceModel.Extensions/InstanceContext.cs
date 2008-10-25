using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.ServiceModel
{
    // Type safe InstanceContext
    public class InstanceContext<T>
    {
        InstanceContext context;

        public InstanceContext(T implementation)
        {
            context = new InstanceContext(implementation);
        }

        public InstanceContext Context
        {
            get { return context; }
        }

        public T ServiceInstance
        {
            get { return (T)context.GetServiceInstance(); }
        }
    }
}
