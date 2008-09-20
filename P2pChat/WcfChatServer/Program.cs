using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.PeerResolvers;
using System.ServiceModel.Description;

namespace WcfChatServer
{
    class Program
    {
        static void Main(string[] args)
        {
            CustomPeerResolverService crs = new CustomPeerResolverService();
            crs.ControlShape = false;
            
            // Create a new service host
            ServiceHost host = new ServiceHost(crs);

            // Open the custom resolver service and wait 
            crs.Open();
            host.Open();

            Console.WriteLine("Custom resolver service started.");
            Console.WriteLine("Press <Esc> to stop the service.");
            do {/*wait*/} while (Console.ReadKey(true).Key != ConsoleKey.Escape);
        }
    }
}
