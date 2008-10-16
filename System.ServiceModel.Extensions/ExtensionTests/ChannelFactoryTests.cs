using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.ServiceModel.Test
{
    /// <summary>
    /// Summary description for ChannelFactoryTests
    /// </summary>
    [TestClass]
    public class ChannelFactoryTests
    {
        public ChannelFactoryTests()
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


        [TestMethod]
        public void CreateChannelTest()
        {
            ITestContract channel = ChannelFactory<TestService, ITestContract>.CreateChannel();
            Assert.AreEqual<string>("MyResult", channel.MyOperation());
            ChannelFactory<TestService, ITestContract>.CloseChannel(channel);
        }

        [TestMethod]
        public void CreateMultipleChannelsTest()
        {
            ITestContract channel1 = ChannelFactory<TestService, ITestContract>.CreateChannel();
            ITestContract channel2 = ChannelFactory<TestService, ITestContract>.CreateChannel();
            Assert.AreNotSame(channel1, channel2);
            Assert.AreEqual<string>("MyResult", channel1.MyOperation());
            Assert.AreEqual<string>("MyResult", channel2.MyOperation());
            ChannelFactory<TestService, ITestContract>.CloseChannel(channel1);
            ChannelFactory<TestService, ITestContract>.CloseChannel(channel2);
        }

        [TestMethod]
        public void CreateChannel_Binding()
        {
            ITestContract channel1 = ChannelFactory<TestService, ITestContract>.CreateChannel();
            ITestContract channel2 = ChannelFactory<TestService, ITestContract>.CreateChannel(new WSHttpBinding(), "http://localhost");
            Assert.AreNotSame(channel1, channel2);
            Assert.AreEqual<string>("MyResult", channel1.MyOperation());
            Assert.AreEqual<string>("MyResult", channel2.MyOperation());
            ChannelFactory<TestService, ITestContract>.CloseChannel(channel1);
            ChannelFactory<TestService, ITestContract>.CloseChannel(channel2);
        }
    }


}
