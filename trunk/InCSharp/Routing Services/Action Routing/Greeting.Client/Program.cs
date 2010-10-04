using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Greeting.Client
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            const string routerAddress = "http://localhost:8001/router";
            Binding routerBinding = new BasicHttpBinding();

            var helloClient = ChannelFactory<IGreetingService>.CreateChannel(routerBinding,
                                                                             new EndpointAddress(routerAddress));
            var helloResult = helloClient.SayHello();
            Console.WriteLine(helloResult);
            ((ICommunicationObject) helloClient).Close();

            var goodbyeClient = ChannelFactory<IGreetingService>.CreateChannel(routerBinding,
                                                                               new EndpointAddress(routerAddress));
            var goodbyeResult = goodbyeClient.SayGoodbye();
            Console.WriteLine(goodbyeResult);
            ((ICommunicationObject) goodbyeClient).Close();
        }
    }
}