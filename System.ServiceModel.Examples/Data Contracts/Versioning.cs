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
    [TestClass]
    public class ExtensibleDataObjectExample
    {
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
            string xml = DataContractSerializer<Person>.Serialize(p);

            // Deserialize as Contact & expect Address to be default
            Contact c = DataContractSerializer<Contact>.Deserialize(xml);
            Assert.AreEqual("Mark", c.Name);
            Assert.AreEqual(default(string), c.Address);

            // Reserialize Contact and Deserialize as Person
            // Expect Name & Age to have been preserved
            xml = DataContractSerializer<Contact>.Serialize(c);
            p = DataContractSerializer<Person>.Deserialize(xml);
            Assert.AreEqual("Mark", p.Name);
            Assert.AreEqual(35, p.Age);
        }

        [TestMethod]
        public void FromContactToPersonAndBack()
        {
            // Serialize new Contact
            Contact c = new Contact() { Name = "Mark", Address = "1234 South Main St." };
            string xml = DataContractSerializer<Contact>.Serialize(c);

            // Deserialize as Person & expect Age to be defaul
            Person p = DataContractSerializer<Person>.Deserialize(xml);
            Assert.AreEqual("Mark", c.Name);
            Assert.AreEqual(default(int), p.Age);

            // Reserialize Person and Deserialize as Contact
            // Expect Name to have been preserved, 
            // Expect Address to be null because Person does 
            // not implement IExtensibleDataObject
            xml = DataContractSerializer<Person>.Serialize(p);
            c = DataContractSerializer<Contact>.Deserialize(xml);
            Assert.AreEqual("Mark", c.Name);
            Assert.IsNull(c.Address);
        }
    }
}
