using System;
using System.ServiceModel;
using System.Diagnostics;

namespace SelfHosting {

    class Program {

        static void Main(string[] args) {
            ServiceHost host = new ServiceHost(typeof(MyService));
            host.Open();

            Process.Start(@"D:\Program Files\Internet Explorer\IEXPLORE.EXE", "http://localhost:8000");
            Console.ReadKey(true);

            host.Close();
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
