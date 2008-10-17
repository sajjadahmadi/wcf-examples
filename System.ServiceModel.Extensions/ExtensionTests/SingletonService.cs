using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel.Channels;

namespace System.ServiceModel.Test
{
    [ServiceContract(SessionMode = SessionMode.Required)]
    interface ISessionRequired
    {
        [OperationContract]
        void IncrementCounter();
    }
    [ServiceContract(SessionMode = SessionMode.NotAllowed)]
    interface ISessionNotAllowed
    {
        [OperationContract]
        void IncrementCounter();
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    class SingletonCounter : ISessionRequired, ISessionNotAllowed, IDisposable
    {
        int counter = 0;

        public int Counter
        {
            get { return counter; }
            set { counter = value; }
        }

        public SingletonCounter()
        {
            Trace.WriteLine("MySingleton.MySingleton()");
        }

        void ISessionRequired.IncrementCounter()
        {
            counter++;
            Trace.WriteLine("Counter = " + counter);
        }

        void ISessionNotAllowed.IncrementCounter()
        {
            counter++;
            Trace.WriteLine("Counter = " + counter);
        }

        public void Dispose()
        {
            Trace.WriteLine("MySingleton.Dispose()");
        }
    }

    [TestClass]
    public class SingletonServiceTests
    {
        [TestMethod]
        public void SingletonSessionsTests()
        {
            Binding binding = new WSHttpBinding();
            ServiceHost<SingletonCounter> host = new ServiceHost<SingletonCounter>("http://localhost/");

            host.AddServiceEndpoint(typeof(ISessionRequired), binding, "withSession");
            host.AddServiceEndpoint(typeof(ISessionNotAllowed), binding, "woutSession");

            host.Open();

            ISessionRequired withSession =
                ChannelFactory<ISessionRequired>.CreateChannel(
                    binding,
                    new EndpointAddress("http://localhost/withSession"));
            withSession.IncrementCounter();
            ((ICommunicationObject)withSession).Close();

            ISessionNotAllowed woutSession =
                ChannelFactory<ISessionNotAllowed>.CreateChannel(
                    binding,
                    new EndpointAddress("http://localhost/woutSession"));
            woutSession.IncrementCounter();
            ((ICommunicationObject)woutSession).Close();

            host.Close();
        }

        [TestMethod]
        public void SingletonInitialization()
        {
            // Initialize Counter
            SingletonCounter myCounter = new SingletonCounter();
            myCounter.Counter = 5;

            // Host
            Binding binding = new WSHttpBinding();
            ServiceHost<SingletonCounter> host = 
                new ServiceHost<SingletonCounter>(myCounter, "http://localhost/");
            host.AddServiceEndpoint(typeof(ISessionRequired), binding, "Counter");
            host.Open();

            // Client
            ISessionRequired proxy = ChannelFactory<ISessionRequired>.CreateChannel(binding, new EndpointAddress("http://localhost/Counter"));
            proxy.IncrementCounter();
            ((ICommunicationObject)proxy).Close();

            Debugger.Break();
        }
    }
}
