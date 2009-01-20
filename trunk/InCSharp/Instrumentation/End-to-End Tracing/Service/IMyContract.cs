using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace WcfExamples.EndToEndTracing.Service
{
    [ServiceContract(Name = "MyService", Namespace = "http://tempuri.org")]
    public interface IMyContract
    {
        [OperationContract]
        [FaultContract(typeof(string))]
        string GetHeader(string name, string ns);
    }
}
