using System;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading;

namespace WcfExamples.Hosting
{
    class ConsoleHost
    {
        static AutoResetEvent startedEvent = new AutoResetEvent(false);
        static ServiceHost host;

        static void Main(string[] args) {
            new Thread(new ThreadStart(RunService)).Start();
            startedEvent.WaitOne();

            var iePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Internet Explorer\\IEXPLORE.EXE");
            Process.Start(iePath, "http://localhost:8000/MyService");
        }

        static void RunService() {
            host = new ServiceHost(typeof(MyService));
            host.Open();
            Console.WriteLine("Service Host: {0}", host.State);
            startedEvent.Set();
            Console.WriteLine("Press ENTER to exit.");
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
