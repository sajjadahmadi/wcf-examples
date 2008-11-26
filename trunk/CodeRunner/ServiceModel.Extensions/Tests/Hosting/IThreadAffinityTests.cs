using System.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.ServiceModel.Channels;
using System;

namespace CodeRunner.ServiceModel.Test
{
    /// <summary>
    ///This is a test class for IThreadAffinity and is intended
    ///to contain all IThreadAffinity Unit Tests
    ///</summary>
    [TestClass()]
    public class IThreadAffinityTests
    {

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        [ServiceContract]
        interface IMyContract
        {
            [OperationContract]
            string GetThreadName();
        }

        [ServiceBehavior(UseSynchronizationContext = true)]
        class MyService : IMyContract
        {
            public string GetThreadName()
            {
                return Thread.CurrentThread.Name;
            }
        }

        /// <summary>
        /// A test for SetThreadAffinity
        /// </summary>
        [TestMethod()]
        public void SetThreadAffinityTest()
        {
            Assert.IsNull(SynchronizationContext.Current, "There should be no SynchronizationContext yet."); 
            
            Binding binding = new NetNamedPipeBinding();
            string address = "net.pipe://localhost/" + Guid.NewGuid().ToString();

            IThreadAffinity threadAffinity = CreateIThreadAffinity(binding, address);
            threadAffinity.SetThreadAffinity("Test Thread");

            using (ServiceHost<MyService> host = (threadAffinity as ServiceHost<MyService>))
            {
                host.Open();
                IInProcFactory factory = (host as IInProcFactory);
                IMyContract channel = factory.CreateChannel<IMyContract>(binding, address);
                Assert.AreEqual("Test Thread", channel.GetThreadName());
                InProcFactory.CloseChannel<IMyContract>(channel);
            }
        }

        internal virtual IThreadAffinity CreateIThreadAffinity(Binding binding, string address)
        {
            Assert.IsNull(SynchronizationContext.Current);
            ServiceHost<MyService> host = new ServiceHost<MyService>();

            host.AddServiceEndpoint<IMyContract>(binding, address);

            return host;
        }
    }
}
