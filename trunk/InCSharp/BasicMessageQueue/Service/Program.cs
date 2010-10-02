using System;
using System.Messaging;
using System.ServiceModel;

namespace QueuedCalls.Service
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            const string queueName = ".\\private$\\MessageQueue";
            if (!MessageQueue.Exists(queueName))
                MessageQueue.Create(queueName, false);

            using (var host = new ServiceHost(typeof(MessagingService)))
            {
                host.Open();

                Console.WriteLine("Host ready.  Press <ENTER> to exit.");
                Console.ReadLine();
            }
        }
    }
}