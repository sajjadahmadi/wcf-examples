using System.Diagnostics;
using System.ServiceModel.Channels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ServiceModel.Examples
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
    [ServiceContract]
    interface ICounter : ISessionRequired
    {
        [OperationContract]
        int GetCurrentValue();
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    class SingletonCounter : ISessionRequired, ISessionNotAllowed, IDisposable, ICounter
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

        #region ICounter Members

        public int GetCurrentValue()
        {
            return counter;
        }

        #endregion
    }

    [TestClass]
    public class SingletonServiceTests
    {
        [TestMethod]
        public void SingletonSessionsTests()
        {
            Binding binding = new WSHttpBinding();
            using (ServiceHost<SingletonCounter> host =
                      new ServiceHost<SingletonCounter>("http://localhost/"))
            {
                host.AddServiceEndpoint<ISessionRequired>(binding, "withSession");
                host.AddServiceEndpoint<ISessionNotAllowed>(binding, "woutSession");

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

            }
        }

        [TestMethod]
        public void SingletonInitialization()
        {
            // Initialize Counter
            SingletonCounter myCounter = new SingletonCounter();
            myCounter.Counter = 5;

            string address = "http://localhost/" + Guid.NewGuid().ToString();
            Binding binding = new WSHttpBinding();
            using (ServiceHost<SingletonCounter> host =
                 new ServiceHost<SingletonCounter>(myCounter))
            {
                // Host
                host.AddServiceEndpoint<ICounter>(binding, address);
                host.Open();

                // Client
                ICounter counter = host.CreateChannel<ICounter>(binding, address);
                counter.IncrementCounter();
                Assert.AreEqual(6, counter.GetCurrentValue());
                ((ICommunicationObject)counter).Close();
            }
        }
    }
}
