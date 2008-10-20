using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace CounterServiceLibrary
{
    [ServiceBehavior(InstanceContextMode=InstanceContextMode.PerCall)]
    public class PerCallCounter : ICounterService
    {
        int currentCount = 0;

        public int IncrementAndReturnCount()
        {
            return currentCount += 1;
        }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class PerSessionCounter : ICounterService
    {
        int currentCount = 0;

        public int IncrementAndReturnCount()
        {
            return currentCount += 1;
        }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class SingletonCounter : ICounterService
    {
        int currentCount = 0;

        public int IncrementAndReturnCount()
        {
            return currentCount += 1;
        }
    }
}
