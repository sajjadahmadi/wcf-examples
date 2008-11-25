using CodeRunner.ServiceModel.ThreadAffinity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace CodeRunner.ServiceModel.ThreadAffinity.Test
{
    /// <summary>
    /// This is a test class for AffinitySynchronizer and is intended
    /// to contain all AffinitySynchronizer Unit Tests
    ///</summary>
    [TestClass()]
    public class AffinitySynchronizerTests
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
        /// Baseline understanding
        /// </summary>
        [TestMethod]
        public void WithoutAffinitySynchronizer()
        {
            Assert.IsNull(SynchronizationContext.Current);
            using (ServiceHost<MyService> host = new ServiceHost<MyService>())
            {
                Binding binding = new NetNamedPipeBinding();
                string address = "net.pipe://localhost/" + Guid.NewGuid().ToString();
                host.AddServiceEndpoint<IMyContract>(binding, address);
                host.Open();
                IMyContract channel = host.CreateChannel<IMyContract>(binding, address);
                Assert.IsNull(channel.GetThreadName());
                InProcFactory.CloseChannel<IMyContract>(channel);
            }
        }

        /// <summary>
        /// Basic test for "installing" AffinitySynchronizer
        /// </summary>
        [TestMethod]
        public void WithAffinitySynchronizer()
        {
            SynchronizationContext context = new AffinitySynchronizer("Test Thread");
            SynchronizationContext.SetSynchronizationContext(context);
            Assert.IsNotNull(SynchronizationContext.Current);

            using (context as IDisposable)
            {
                IMyContract channel = InProcFactory.CreateChannel<MyService, IMyContract>();
                Assert.AreEqual("Test Thread", channel.GetThreadName());
                InProcFactory.CloseChannel<IMyContract>(channel);
            }
        }
    }
}
