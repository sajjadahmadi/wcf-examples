using System;
using System.ServiceModel;

namespace Discovery.Service
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using (var host = new ServiceHost(typeof(DiscoverableService), new Uri("http://localhost:8888/")))
            {
                host.Open();

                Console.WriteLine("Press <ENTERT> to exit.");
                Console.ReadLine();
            }
        }
    }
}