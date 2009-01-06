using System;
using System.Diagnostics;
using System.ServiceModel;

namespace WcfExamples
{
    class SelfHosting
    {
        static void Main(string[] args) {
            ServiceHost host = new ServiceHost(typeof(MyService));
            host.Open();
            Console.WriteLine("Service Host: {0}", host.State);

            var iePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Internet Explorer\\IEXPLORE.EXE");
            Process.Start(iePath, "http://localhost:8000");
            Console.ReadKey(true);

            host.Close();
        }
    }

    [ServiceContract]
    interface IMyContract
    {

        [OperationContract]
        string MyOperation();
    }

    class MyService : IMyContract
    {

        public string MyOperation() {
            return "MyResult";
        }
    }
}
