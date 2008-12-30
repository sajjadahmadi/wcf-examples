using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ServiceModel.Examples
{
    enum EventType
    {
        Event1 = 1,
        Event2 = 2,
        Event3 = 4,
        AllEvents = Event1 | Event2 | Event3
    }

    interface IMyEvents
    {
        [OperationContract(IsOneWay = true)]
        void OnEvent1();
        [OperationContract(IsOneWay = true)]
        void OnEvent2(int number);
        [OperationContract(IsOneWay = true)]
        void OnEvent3(int number, string text);
    }

    [ServiceContract]
    interface ISubscriptionService
    {
        [OperationContract]
        void Subscribe(EventType mask);
        [OperationContract]
        void Unsubscribe(EventType mask);
    }

    [ServiceContract(CallbackContract = typeof(IMyEvents))]
    interface IContractWithEvents : ISubscriptionService
    {
        [OperationContract]
        void FireEvents(EventType eventType);
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    class MyPublisher : IContractWithEvents
    {
        public delegate void GenericEventHandler();
        public delegate void GenericEventHandler<T>(T t);
        public delegate void GenericEventHandler<T, U>(T t, U u);

        static GenericEventHandler event1 = delegate { };
        static GenericEventHandler<int> event2 = delegate { };
        static GenericEventHandler<int, string> event3 = delegate { };

        #region IContractWithEvents Members

        public void Subscribe(EventType mask)
        {
            IMyEvents subscriber = OperationContext.Current.GetCallbackChannel<IMyEvents>();

            if ((mask & EventType.Event1) == EventType.Event1)
            { event1 += subscriber.OnEvent1; }
            if ((mask & EventType.Event2) == EventType.Event2)
            { event2 += subscriber.OnEvent2; }
            if ((mask & EventType.Event3) == EventType.Event3)
            { event3 += subscriber.OnEvent3; }
        }

        public void Unsubscribe(EventType mask)
        {
            IMyEvents subscriber = OperationContext.Current.GetCallbackChannel<IMyEvents>();

            if ((mask & EventType.Event1) == EventType.Event1)
            { event1 -= subscriber.OnEvent1; }
            if ((mask & EventType.Event2) == EventType.Event2)
            { event2 -= subscriber.OnEvent2; }
            if ((mask & EventType.Event3) == EventType.Event3)
            { event3 -= subscriber.OnEvent3; }
        }

        public void FireEvents(EventType eventType)
        {
            switch (eventType)
            {
                case EventType.Event1:
                    { event1(); break; }
                case EventType.Event2:
                    { event2(30); break; }
                case EventType.Event3:
                    { event3(60, "Some Text"); break; }
                case EventType.AllEvents:
                    {
                        event1();
                        event2(50);
                        event3(100, "All");
                        break;
                    }
                default:
                    { throw new InvalidOperationException("Unknown event!"); }
            }
        }

        #endregion
    }

    class MySubscriber : IMyEvents
    {
        public EventType EventFired;
        public int Number;
        public string Text;

        #region IMyEvents Members

        public void OnEvent1()
        { EventFired |= EventType.Event1; }

        public void OnEvent2(int number)
        {
            Number = number;
            EventFired |= EventType.Event2;
        }

        public void OnEvent3(int number, string text)
        {
            Number = number;
            Text = text;
            EventFired |= EventType.Event3;
        }

        #endregion
    }

    [TestClass]
    public class Events
    {
        [TestMethod]
        public void SubscribeToEvents()
        {
            string address = "net.pipe://localhost/";
            using (ServiceHost<MyPublisher> host = new ServiceHost<MyPublisher>())
            {
                host.AddServiceEndpoint<IContractWithEvents>(new NetNamedPipeBinding(), address);
                host.Open();

                MySubscriber client = new MySubscriber();
                IContractWithEvents channel = DuplexChannelFactory<IContractWithEvents, IMyEvents>
                    .CreateChannel(client, new NetNamedPipeBinding(), new EndpointAddress(address));

                // Subscribed to All, but only fired Event1
                channel.Subscribe(EventType.AllEvents);
                channel.FireEvents(EventType.Event1);
                Assert.AreEqual(EventType.Event1, (client.EventFired & EventType.Event1));
                Assert.AreEqual(default(int), client.Number);
                Assert.AreEqual(default(string), client.Text);

                channel.Unsubscribe(EventType.AllEvents);

                // Fired All, but only subscribed to Event2
                channel.Subscribe(EventType.Event2);
                channel.FireEvents(EventType.AllEvents);
                Assert.AreEqual(EventType.Event2, (client.EventFired & EventType.Event2));
                Assert.AreNotEqual(EventType.AllEvents, (client.EventFired & EventType.AllEvents));
                Assert.AreEqual(50, client.Number);
                Assert.AreEqual(default(string), client.Text);

                channel.Unsubscribe(EventType.AllEvents);

                // Subscribed to All, Fired All
                channel.Subscribe(EventType.AllEvents);
                channel.FireEvents(EventType.AllEvents);
                Assert.AreEqual(EventType.AllEvents, (client.EventFired & EventType.AllEvents));
                Assert.AreEqual(100, client.Number);
                Assert.AreEqual("All", client.Text);

                ((ICommunicationObject)channel).Close();
            }

        }
    }
}
