using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Client
{
    // Contract
    [ServiceContract]
    interface IStreamServiceContract
    {
        [OperationContract]
        Stream EchoStream(Stream stream);
    }

    // Service
    class StreamService : IStreamServiceContract
    {
        public Stream EchoStream(Stream stream)
        {
            return stream;
        }
    }

    // Client
    class StreamServiceContractClient : ClientBase<IStreamServiceContract>, IStreamServiceContract
    {
        public StreamServiceContractClient(Binding binding, string address)
            : base(binding, new EndpointAddress(address))
        { }

        public Stream EchoStream(Stream stream)
        {
            return Channel.EchoStream(stream);
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            string address = "http://localhost:8731/" + Guid.NewGuid().ToString();
            BasicHttpBinding streamedBinding = new BasicHttpBinding();
            streamedBinding.TransferMode = TransferMode.Streamed;
            using (ServiceHost host = new ServiceHost(typeof(StreamService)))
            {
                host.AddServiceEndpoint(typeof(IStreamServiceContract), streamedBinding, address);
                host.Open();

                Stream stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream);
                writer.WriteLine("This is a test...");
                writer.Flush();
                stream.Position = 0;

                using (StreamServiceContractClient client = 
                    new StreamServiceContractClient(streamedBinding, address))
                {
                    client.Open();
                    Stream returnStream = client.EchoStream(stream);
                    StreamReader reader = new StreamReader(returnStream);
                    Console.WriteLine(reader.ReadToEnd());
                    client.Close();
                }

                host.Close();
            }
            Console.ReadKey();
        }
    }
}
