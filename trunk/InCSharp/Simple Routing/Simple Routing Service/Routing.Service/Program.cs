using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Routing;

namespace Routing.Service
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            const string routerAddress = "http://localhost:8001/router";
            using (var routerHost = new ServiceHost(typeof(RoutingService), new Uri(routerAddress)))
            {
                Binding routerBinding = new BasicHttpBinding();
                routerHost.AddServiceEndpoint(typeof(IRequestReplyRouter), routerBinding, string.Empty);

                Binding greetingBinding = new NetTcpBinding();
                const string greetingBase = "net.tcp://localhost:8000/GreetingService/";

                var contractDescription = ContractDescription.GetContract(typeof(IRequestReplyRouter));
                var helloEndpoint = new ServiceEndpoint(contractDescription, greetingBinding,
                                                        new EndpointAddress(greetingBase + "Hello"));
                var goodbyeEndpoint = new ServiceEndpoint(contractDescription, greetingBinding,
                                                          new EndpointAddress(greetingBase + "Goodbye"));

                var routerConfig = new RoutingConfiguration();
                IEnumerable<ServiceEndpoint> helloEndpoints = new List<ServiceEndpoint>
                                                                  {
                                                                      helloEndpoint,
                                                                  };
                routerConfig.FilterTable.Add(new ActionMessageFilter("Hello"), helloEndpoints);
                IEnumerable<ServiceEndpoint> goodbyeEndpoints = new List<ServiceEndpoint>
                                                                    {
                                                                        goodbyeEndpoint,
                                                                    };
                routerConfig.FilterTable.Add(new ActionMessageFilter("Goodbye"), goodbyeEndpoints);
                routerHost.Description.Behaviors.Add(new RoutingBehavior(routerConfig));

                routerHost.Open();

                Console.WriteLine("Routing service ready.");
                do
                {
                    Console.WriteLine("Press <ESC> to exit.");
                } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
                Console.WriteLine("Exiting...");
                routerHost.Close();
            }

        }
    }
}