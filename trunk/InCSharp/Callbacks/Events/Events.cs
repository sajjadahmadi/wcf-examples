using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Serialization;
using System.ServiceModel;
using System.Runtime.Serialization;
using System;

namespace CodeRunner.Service
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
	class PublisherService : IPublisherService
	{
		public delegate void GenericEventHandler();
		public delegate void GenericEventHandler<T>(T t);
		public delegate void GenericEventHandler<T, U>(T t, U u);

		static GenericEventHandler event1 = delegate { };
		static GenericEventHandler<int> event2 = delegate { };
		static GenericEventHandler<int, string> event3 = delegate { };

		public void Subscribe(EventType mask)
		{
			IPublisherEvents subscriber = OperationContext.Current.GetCallbackChannel<IPublisherEvents>();

			if ((mask & EventType.Event1) == EventType.Event1)
			{ event1 += subscriber.OnEvent1; }
			if ((mask & EventType.Event2) == EventType.Event2)
			{ event2 += subscriber.OnEvent2; }
			if ((mask & EventType.Event3) == EventType.Event3)
			{ event3 += subscriber.OnEvent3; }
		}

		public void Unsubscribe(EventType mask)
		{
			IPublisherEvents subscriber = OperationContext.Current.GetCallbackChannel<IPublisherEvents>();

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
	}

	[ServiceContract(CallbackContract = typeof(IPublisherEvents))]
	interface IPublisherService : ISubscriptionOperations
	{
		[OperationContract]
		void FireEvents(EventType eventType);
	}

	[ServiceContract]
	interface ISubscriptionOperations
	{
		[OperationContract]
		void Subscribe(EventType mask);
		[OperationContract]
		void Unsubscribe(EventType mask);
	}

	interface IPublisherEvents
	{
		[OperationContract(IsOneWay = true)]
		void OnEvent1();
		[OperationContract(IsOneWay = true)]
		void OnEvent2(int number);
		[OperationContract(IsOneWay = true)]
		void OnEvent3(int number, string text);
	}

	[DataContract]
	enum EventType
	{
		[EnumMember]
		Event1 = 1,

		[EnumMember]
		Event2 = 2,

		[EnumMember]
		Event3 = 4,

		[EnumMember]
		AllEvents = Event1 | Event2 | Event3
	}
}




namespace CodeRunner.Client
{
	// Pretend like we're using a generated proxy
	using CodeRunner.Service;

	class EventSubscriber : IPublisherEvents
	{
		public EventType EventFired;
		public int Number;
		public string Text;

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
	}

	[TestClass]
	public class Events
	{
		#region Additional test attributes
		static string address = "net.pipe://localhost/";
		static ServiceHost<PublisherService> host;

		[ClassInitialize]
		public static void StartHost(TestContext context)
		{
			host = new ServiceHost<PublisherService>();
			host.AddServiceEndpoint<IPublisherService>(new NetNamedPipeBinding(), address);
			host.Open();
		}

		[ClassCleanup]
		public static void StopHost()
		{
			using (host) { /* Disposed here */ }
		}
		#endregion

		[TestMethod]
		public void SubscribeToAll_FireEvent1()
		{
			EventSubscriber subscriber = new EventSubscriber();
			IPublisherService proxy = DuplexChannelFactory<IPublisherService, IPublisherEvents>
				 .CreateChannel(subscriber, new NetNamedPipeBinding(), new EndpointAddress(address));

			// Subscribe to All, but only fire Event1
			proxy.Subscribe(EventType.AllEvents);
			proxy.FireEvents(EventType.Event1);

			Assert.AreEqual(EventType.Event1, (subscriber.EventFired & EventType.Event1));
			Assert.AreEqual(default(int), subscriber.Number);
			Assert.AreEqual(default(string), subscriber.Text);

			CloseProxy(proxy);
		}

		[TestMethod]
		public void SubscribeToOne_FireAll()
		{
			EventSubscriber subscriber = new EventSubscriber();
			IPublisherService proxy = DuplexChannelFactory<IPublisherService, IPublisherEvents>
				 .CreateChannel(subscriber, new NetNamedPipeBinding(), new EndpointAddress(address));

			// Fired All, but only subscribed to Event2
			proxy.Subscribe(EventType.Event2);
			proxy.FireEvents(EventType.AllEvents);

			Assert.AreEqual(EventType.Event2, (subscriber.EventFired & EventType.Event2));
			Assert.AreNotEqual(EventType.AllEvents, (subscriber.EventFired & EventType.AllEvents));
			Assert.AreEqual(50, subscriber.Number);
			Assert.AreEqual(default(string), subscriber.Text);

			CloseProxy(proxy);
		}

		[TestMethod]
		public void SubscribeToAll_FireAll()
		{
			EventSubscriber subscriber = new EventSubscriber();
			IPublisherService proxy = DuplexChannelFactory<IPublisherService, IPublisherEvents>
				 .CreateChannel(subscriber, new NetNamedPipeBinding(), new EndpointAddress(address));

			// Subscribe to All, Fire All
			proxy.Subscribe(EventType.AllEvents);
			proxy.FireEvents(EventType.AllEvents);

			Assert.AreEqual(EventType.AllEvents, (subscriber.EventFired & EventType.AllEvents));
			Assert.AreEqual(100, subscriber.Number);
			Assert.AreEqual("All", subscriber.Text);

			CloseProxy(proxy);
		}

		private static void CloseProxy(IPublisherService proxy)
		{
			proxy.Unsubscribe(EventType.AllEvents);
			((ICommunicationObject)proxy).Close();
		}
	}
}
