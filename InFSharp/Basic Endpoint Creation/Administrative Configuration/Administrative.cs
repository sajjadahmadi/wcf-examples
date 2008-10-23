using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace System.ServiceModel.Examples
{
    class AdministrativeEndpoints
    {
        static void Main(string[] args)
        {
            using (ServiceHost host = new ServiceHost(typeof(MyService)))
            {
                host.Open();
                Debug.Assert(host.State == CommunicationState.Opened);
                Debug.Assert(host.Description.Endpoints.Count == 6);
            }
        }

    } // End AdministrativeEndpoints

    [ServiceContract]
    interface IMyContract
    {

        [OperationContract]
        string MyOperation();
    }

    // Note: Service cannot be a subclass when configuring via App.config
    public class MyService : IMyContract
    {

        public string MyOperation()
        {
            return "MyResult";
        }
    }
}
