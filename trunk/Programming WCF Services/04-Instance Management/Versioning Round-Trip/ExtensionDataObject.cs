using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel;
using System.ServiceModel.Extensions;
using System.Runtime.Serialization;

namespace Versioning_Round_Trip
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class ExtensionDataObject
    {
        public ExtensionDataObject()
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


        [DataContract]
        public class Person
        {
            public string Name { get; set; }
        }

        [DataContract]
        public class Contact
        {
            public string Name { get; set; }
            public string Address { get; set; }
        }


        [TestMethod]
        public void TestMethod1()
        {
            //Person p = new Person() { Name="Mark" };
            //string xml = Helper.Serialize<Person>(p);

            //Contact c = Helper.Deserialize<Contact>(xml);
            //Assert.AreEqual("Mark", c.Name);
            //Assert.IsNull(c.Address);
        }
    }
}
