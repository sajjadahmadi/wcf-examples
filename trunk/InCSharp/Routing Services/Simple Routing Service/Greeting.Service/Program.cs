using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Greeting.Service
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            const string greetingBase = "net.tcp://localhost:8000/GreetingService/";
            using (var greetingHost = new ServiceHost(typeof(GreetingService), new Uri(greetingBase)))
            {
                Binding greetingBinding = new NetTcpBinding();
                greetingHost.AddServiceEndpoint(typeof(IHelloService), greetingBinding, "Hello");
                greetingHost.AddServiceEndpoint(typeof(IGoodbyeService), greetingBinding, "Goodbye");
                greetingHost.Open();

                Console.WriteLine("Greeting service ready.");
                do
                {
                    Console.WriteLine("Press <ESC> to exit.");
                } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
                Console.WriteLine("Exiting...");
                greetingHost.Close();
            }
        }
    }
}