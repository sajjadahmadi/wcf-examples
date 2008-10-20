using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel;
using System.ServiceModel.Extensions;
using System.Runtime.Serialization;

namespace System.ServiceModel.Examples
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class ExtensionDataObjectExample
    {
        public ExtensionDataObjectExample()
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


        [DataContract(Name = "Person")]
        public class Person
        {
            [DataMember]
            public string Name { get; set; }
            [DataMember]
            public int Age { get; set; }
        }

        [DataContract(Name = "Person")]
        public class Contact : IExtensibleDataObject
        {
            [DataMember]
            public string Name { get; set; }
            [DataMember]
            public string Address { get; set; }

            #region IExtensibleDataObject Members
            ExtensionDataObject _extensionData;
            ExtensionDataObject IExtensibleDataObject.ExtensionData
            {
                get { return _extensionData; }
                set { _extensionData = value; }
            }
            #endregion
        }


        [TestMethod]
        public void FromPersonToContactAndBack()
        {
            // Serialize new Person
            Person p = new Person() { Name = "Mark", Age = 35 };
            string xml = Helper.Serialize<Person>(p);

            // Deserialize as Contact & expect Address to be default
            Contact c = Helper.Deserialize<Contact>(xml);
            Assert.AreEqual("Mark", c.Name);
            Assert.AreEqual(default(string), c.Address);

            // Reserialize Contact and Deserialize as Person
            // Expect Name & Age to have been preserved
            xml = Helper.Serialize<Contact>(c);
            p = Helper.Deserialize<Person>(xml);
            Assert.AreEqual("Mark", p.Name);
            Assert.AreEqual(35, p.Age);
        }

        [TestMethod]
        public void FromContactToPersonAndBack()
        {
            // Serialize new Contact
            Contact c = new Contact() { Name = "Mark", Address = "1234 South Main St." };
            string xml = Helper.Serialize<Contact>(c);

            // Deserialize as Person & expect Age to be defaul
            Person p = Helper.Deserialize<Person>(xml);
            Assert.AreEqual("Mark", c.Name);
            Assert.AreEqual(default(int), p.Age);

            // Reserialize Person and Deserialize as Contact
            // Expect Name to have been preserved, 
            // Expect Address to be null because Person does 
            // not implement IExtensibleDataObject
            xml = Helper.Serialize<Person>(p);
            c = Helper.Deserialize<Contact>(xml);
            Assert.AreEqual("Mark", c.Name);
            Assert.IsNull(c.Address);
        }
    }
}
