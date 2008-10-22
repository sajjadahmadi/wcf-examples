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
    public class UnitTest1
    {
        public UnitTest1()
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
            void FireAndForget();
        }

        [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
        class MyService : IMyContract
        {
            public void FireAndForget()
            { 
                Trace.WriteLine("Operation fired");
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
                service.FireAndForget();
                ((ICommunicationObject)service).Close();
            }
        }
    }
}
