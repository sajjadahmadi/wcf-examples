using System;
using System.Messaging;

namespace QueuedCalls.Client
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using (var client = new MessagingClient())
                client.SendMessage("test message");

            Console.WriteLine("Message sent.  Press <ENTER> to exit.");
            Console.ReadLine();
        }
    }
}