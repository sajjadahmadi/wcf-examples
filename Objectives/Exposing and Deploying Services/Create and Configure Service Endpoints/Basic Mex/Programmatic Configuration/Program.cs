using System.ServiceModel;
using System.ServiceModel.Description;

namespace Programmatic_Configuration {

    class Program {

        static void Main(string[] args) {
            using (ServiceHost host = new ServiceHost(typeof(MyService), new System.Uri("http://localhost"))) {
                // Not relevant to this example but necessary to get it to run
                host.AddServiceEndpoint(typeof(IMyContract), new WSHttpBinding(), "MyService");


                ServiceMetadataBehavior metadataBehavior;
                metadataBehavior = host.Description.Behaviors.Find<ServiceMetadataBehavior>();
                if (metadataBehavior == null) {
                    metadataBehavior = new ServiceMetadataBehavior();
                    metadataBehavior.HttpGetEnabled = true;
                    host.Description.Behaviors.Add(metadataBehavior);
                }
                host.Open();
                var path = System.Environment.GetEnvironmentVariable("ProgramFiles");
                System.Diagnostics.Process.Start(path + @"\internet explorer\iexplore.exe", "http://localhost");
                System.Console.ReadKey(true);
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
