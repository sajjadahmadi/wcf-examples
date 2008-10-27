using System;
using Client.StreamServiceReference;
using System.IO;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            StreamServiceContractClient client = new StreamServiceContractClient();
            client.Open();

            Stream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.WriteLine("This is a test...");
            writer.Flush();
            stream.Position = 0;
            Stream returnStream = client.EchoStream(stream);

            StreamReader reader = new StreamReader(returnStream);
            Console.WriteLine(reader.ReadToEnd());
            Console.ReadKey();
        }
    }
}
