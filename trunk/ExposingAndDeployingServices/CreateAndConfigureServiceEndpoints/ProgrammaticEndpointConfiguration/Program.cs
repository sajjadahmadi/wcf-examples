using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace ProgrammaticEndpointConfiguration {

    class Program {

        static void Main(string[] args) {
            using (ServiceHost host = new ServiceHost(typeof(MyService))) {

                Binding wsBinding = new WSHttpBinding();
                Binding tcpBinding = new NetTcpBinding();

                host.AddServiceEndpoint(typeof(IMyContract), wsBinding, "http://localhost:8000/MyService");
                host.AddServiceEndpoint(typeof(IMyContract), tcpBinding, "net.tcp://localhost:8001/MyService");
                host.AddServiceEndpoint(typeof(IMyContract), tcpBinding, "net.tcp://localhost:8002/MyService");

                // host base address
                host.AddServiceEndpoint(typeof(IMyContract), wsBinding, "");

                // relative address
                host.AddServiceEndpoint(typeof(IMyContract), wsBinding, "MyService");

                // Absolute address
                host.AddServiceEndpoint(typeof(IMyContract), wsBinding, "http://localhost:8003/MyService");

                host.Open();
            }
        }
    }

    [ServiceContract]
    interface IMyContract {

        [OperationContract]
        string MyOperation();
    }

    class MyService : IMyContract {

        public string MyOperation() {
            return "MyResult";
        }
    }
}
