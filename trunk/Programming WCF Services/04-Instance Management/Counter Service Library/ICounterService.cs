using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace CounterServiceLibrary
{
    [ServiceContract]
    public interface ICounterService
    {
        [OperationContract]
        int IncrementAndReturnCount();
    }
}
