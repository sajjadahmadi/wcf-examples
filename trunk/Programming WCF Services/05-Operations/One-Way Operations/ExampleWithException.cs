using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel;
using System.Diagnostics;

namespace One_Way_Operations
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class ExampleWithException
    {
        public ExampleWithException()
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

        [ServiceContract(SessionMode = SessionMode.Required)]
        interface IMyContract
        {
            [OperationContract(IsOneWay = true)]
            void OneWayCall(bool throwException);
        }

        [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
        class MyService : IMyContract
        {
            public void OneWayCall(bool throwException)
            {
                Trace.WriteLine("Operation called");
                if (throwException)
                    throw new ApplicationException("Exception thrown in a One-Way operation");
            }
        }


        [TestMethod]
        public void TestMethod1()
        {
            string address = "net.pipe://localhost/";
            using (ServiceHost<MyService> host = new ServiceHost<MyService>())
            {
                host.AddServiceEndpoint(typeof(IMyContract), new NetNamedPipeBinding(), address);
                host.Open();

                IMyContract service = ChannelFactory<IMyContract>.CreateChannel(new NetNamedPipeBinding(), new EndpointAddress(address));
                ICommunicationObject comm = (ICommunicationObject)service;
                Assert.AreEqual(CommunicationState.Created, comm.State);

                service.OneWayCall(false);
                Assert.AreEqual(CommunicationState.Opened, comm.State);

                try
                {
                    service.OneWayCall(true);  // Call causes exception inside service
                    service.OneWayCall(false); // Call fails 
                }
                catch (CommunicationException) { };

                Assert.AreEqual(CommunicationState.Faulted, comm.State);

                try
                {
                    // Cannot close because the connection is in a faulted state.
                    ((ICommunicationObject)service).Close();
                    Assert.Fail("Expected Close() to fail.");
                }
                catch (CommunicationObjectFaultedException) { };
            }
        }
    }
}
