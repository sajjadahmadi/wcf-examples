using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Routing;

namespace Simple_Routing_Service
{
    [ServiceContract]
    internal interface IHelloService
    {
        [OperationContract(Action="Hello")]
        string SayHello();
    }

    [ServiceContract]
    internal interface IGoodbyeService
    {
        [OperationContract(Action="Goodbye")]
        string SayGoodbye();
    }

    [ServiceContract]
    interface IComposite
    {
        [OperationContract(Action = "Hello")]
        string SayHello();

        [OperationContract(Action = "Goodbye")]
        string SayGoodbye();
    }

    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    internal class GreetingService : IHelloService, IGoodbyeService
    {
        #region IHelloService Members

        public string SayHello()
        {
            Console.WriteLine("Hello Service call...");
            return "Hello";
        }

        #endregion

        public string SayGoodbye()
        {
            Console.WriteLine("Goodbye Serevice call...");
            return "Goodbye";
        }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            const string greetingBase = "net.tcp://localhost:8000/GreetingService/";
            var greetingHost = new ServiceHost(typeof(GreetingService), new Uri(greetingBase));
            Binding helloBinding = new NetTcpBinding();
            greetingHost.AddServiceEndpoint(typeof(IHelloService), helloBinding, "Hello");
            greetingHost.AddServiceEndpoint(typeof(IGoodbyeService), helloBinding, "Goodbye");
            greetingHost.Open();

            var routerHost = new ServiceHost(typeof(RoutingService));
            Binding routerBinding = new BasicHttpBinding();
            const string routerAddress = "http://localhost:8001/globalRouter";
            routerHost.AddServiceEndpoint(typeof(IRequestReplyRouter), routerBinding, routerAddress);
            var behavior = routerHost.Description.Behaviors.Find<ServiceDebugBehavior>();
            behavior.IncludeExceptionDetailInFaults = true;

            var contractDescription = ContractDescription.GetContract(typeof(IRequestReplyRouter));
            var helloEndpoint = new ServiceEndpoint(contractDescription, helloBinding, new EndpointAddress(greetingBase + "Hello"));
            var goodbyeEndpoint = new ServiceEndpoint(contractDescription, helloBinding, new EndpointAddress(greetingBase + "Goodbye"));

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

            var helloClient = ChannelFactory<IComposite>.CreateChannel(routerBinding, new EndpointAddress(routerAddress));
            var helloResult = helloClient.SayHello();
            Console.WriteLine(helloResult);
            ((ICommunicationObject)helloClient).Close();

            var goodbyeClient = ChannelFactory<IComposite>.CreateChannel(routerBinding, new EndpointAddress(routerAddress));
            var goodbyeResult = goodbyeClient.SayGoodbye();
            Console.WriteLine(goodbyeResult);
            ((ICommunicationObject)goodbyeClient).Close();

            greetingHost.Close();
            routerHost.Close();
        }
    }
}

