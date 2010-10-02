using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using QueuedCalls.Contract;

namespace QueuedCalls.Client
{
    class MessagingClient : ClientBase<IMessagingService>, IMessagingService
    {
        public void SendMessage(string message)
        {
            Channel.SendMessage(message);
        }
    }
}
