using System;
using System.ServiceModel;

namespace SelfHosting {

    class Program {

        static void Main(string[] args) {
            ServiceHost host = new ServiceHost(typeof(MyService));
            host.Open();

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
