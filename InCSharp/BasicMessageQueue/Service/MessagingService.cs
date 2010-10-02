using System;
using System.ServiceModel;
using QueuedCalls.Contract;

namespace QueuedCalls.Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    internal class MessagingService : IMessagingService
    {
        public void SendMessage(string message)
        {
            Console.WriteLine(message);
        }
    }
}