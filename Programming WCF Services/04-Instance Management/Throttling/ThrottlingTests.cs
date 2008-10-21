using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Description;

namespace Throttling
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class ThrottlingTests
    {
        public ThrottlingTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [ServiceContract]
        public interface IThrottlingInformation
        {
            [OperationContract]
            int ReadMaxConcurrentCalls();
            [OperationContract]
            int ReadMaxConcurrentSessions();
            [OperationContract]
            int ReadMaxConcurrentInstances();
        }

        [ServiceContract]
        public interface IPingService
        {
            [OperationContract]
            bool Ping();
        }

        [ServiceBehavior]
        public class ThrottledService : IThrottlingInformation, IPingService
        {

            #region IThrottlingInformation

            public int ReadMaxConcurrentCalls()
            { return Throttle.MaxConcurrentCalls; }
            public int ReadMaxConcurrentSessions()
            { return Throttle.MaxConcurrentSessions; }
            public int ReadMaxConcurrentInstances()
            { return Throttle.MaxConcurrentInstances; }

            #endregion IThrottlingInformation

            private ServiceThrottle Throttle
            {
                get
                {
                    ServiceThrottle serviceThrottle;
                    ChannelDispatcher dispatcher =
                        OperationContext.Current.Host.ChannelDispatchers[0] as ChannelDispatcher;
                    serviceThrottle = dispatcher.ServiceThrottle;
                    return serviceThrottle;
                }
            }

            #region IPingService Members

            public bool Ping()
            { return true; }

            #endregion
        }


        [TestMethod]
        public void ReadThrottlingValues()
        {
            ServiceHost<ThrottledService> host = new ServiceHost<ThrottledService>("http://localhost:8080/");
            host.AddServiceEndpoint(typeof(IThrottlingInformation), new WSHttpBinding(), "");
            host.SetThrottle(12, 34, 56);
            host.Open();

            IThrottlingInformation service = ChannelFactory<IThrottlingInformation>.CreateChannel(
                new WSHttpBinding(),
                new EndpointAddress("http://localhost:8080/"));

            Assert.AreEqual(12, service.ReadMaxConcurrentCalls());
            Assert.AreEqual(34, service.ReadMaxConcurrentSessions());
            Assert.AreEqual(56, service.ReadMaxConcurrentInstances());

            ((ICommunicationObject)service).Close();
            host.Close();
        }

        [TestMethod]
        public void MaxSessions()
        {
            string address = "net.pipe://localhost/";
            ServiceHost<ThrottledService> host = new ServiceHost<ThrottledService>(address);
            NetNamedPipeBinding binding = new NetNamedPipeBinding();
            host.AddServiceEndpoint(typeof(IPingService), binding, "");
            host.SetThrottle(12, 1, 56);
            host.Open();

            IPingService service1 = ChannelFactory<IPingService>.CreateChannel(
                new NetNamedPipeBinding(),
                new EndpointAddress(address));
            Assert.AreEqual(true, service1.Ping());

            try
            {
                IPingService service2 = ChannelFactory<IPingService>.CreateChannel(
                    new NetNamedPipeBinding() { SendTimeout = new TimeSpan(0, 0, 1) },
                    new EndpointAddress(address));
                Assert.AreEqual(true, service2.Ping());
                ((ICommunicationObject)service2).Close();
                Assert.Fail("Expected SendTimeout exception.");
            }
            catch (TimeoutException) { }
            finally
            {
                ((ICommunicationObject)service1).Close();
                host.Close();
            }
        }

        [TestMethod]
        public void BindingMaxConnections()
        {
            string address = "net.pipe://localhost/";
            ServiceHost<ThrottledService> host = new ServiceHost<ThrottledService>(address);
            NetNamedPipeBinding binding = new NetNamedPipeBinding();
            binding.MaxConnections = 1;
            host.AddServiceEndpoint(typeof(IPingService), binding, "");
            host.Open();

            IPingService service1 = ChannelFactory<IPingService>.CreateChannel(
                new NetNamedPipeBinding(),
                new EndpointAddress(address));
            Assert.AreEqual(true, service1.Ping());

            try
            {
                IPingService service2 = ChannelFactory<IPingService>.CreateChannel(
                    new NetNamedPipeBinding() { SendTimeout = new TimeSpan(0, 0, 2) },
                    new EndpointAddress(address));
                Assert.AreEqual(true, service2.Ping());
                ((ICommunicationObject)service2).Close();
                Assert.Fail("Expected SendTimeout exception.");
            }
            catch (TimeoutException) { }
            finally
            {
                ((ICommunicationObject)service1).Close();
                host.Close();
            }
        }
    }
}
