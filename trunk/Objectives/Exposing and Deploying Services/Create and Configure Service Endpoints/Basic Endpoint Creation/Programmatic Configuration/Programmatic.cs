using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Channels;
using System.Diagnostics;

namespace System.ServiceModel.Examples
{
    class Programmatic
    {
        static void Main(string[] args)
        {
            Uri baseAddress = new Uri("http://localhost:8000");
            using (ServiceHost host = new ServiceHost(typeof(MyService), baseAddress))
            {
                Binding wsBinding = new WSHttpBinding();
                Binding tcpBinding = new NetTcpBinding();

                // Base address only
                host.AddServiceEndpoint(typeof(IMyContract), wsBinding, "");

                // Relative to base address
                host.AddServiceEndpoint(typeof(IMyContract), wsBinding, "MyService");

                // Absolute address
                host.AddServiceEndpoint(typeof(IMyContract), wsBinding, "http://localhost:8001/MyService");

                // Can add multiple endpoints of the same type as long as they differ by URL
                host.AddServiceEndpoint(typeof(IMyContract), wsBinding, "http://localhost:8002/MyService");
                host.AddServiceEndpoint(typeof(IMyContract), tcpBinding, "net.tcp://localhost:8003/MyService");
                host.AddServiceEndpoint(typeof(IMyContract), tcpBinding, "net.tcp://localhost:8004/MyService");

                host.Open();
                Debug.Assert(host.State == CommunicationState.Opened);
                Debug.Assert(host.Description.Endpoints.Count == 6);
            }
        }

    } // End Programmatic

    [ServiceContract]
    interface IMyContract
    {

        [OperationContract]
        string MyOperation();
    }

    class MyService : IMyContract
    {

        public string MyOperation()
        {
            return "MyResult";
        }
    }
}
