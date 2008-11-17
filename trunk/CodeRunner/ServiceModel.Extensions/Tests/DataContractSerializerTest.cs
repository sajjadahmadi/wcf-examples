using System.Runtime.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Xml.Linq;

namespace System.ServiceModel.Examples
{
    /// <summary>
    ///This is a test class for DataContractSerializerTest and is intended
    ///to contain all DataContractSerializerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DataContractSerializerTest
    {
        [TestMethod()]
        public void WriteObjectTest()
        {
            XmlObjectSerializer serializer = new DataContractSerializer<TestDataContract>();
            MemoryStream stream = new MemoryStream();
            TestDataContract graph = new TestDataContract();
            serializer.WriteObject(stream, graph);
            string expected = TestDataContract.SER_DATA;
            string actual;
            stream.Position = 0;
            using (TextReader reader = new StreamReader(stream))
            {
                actual = reader.ReadToEnd();
            }
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ReadObjectTest()
        {
            DataContractSerializer<TestDataContract> target = new DataContractSerializer<TestDataContract>();
            Stream stream = new MemoryStream(Encoding.Default.GetBytes(TestDataContract.SER_DATA));
            TestDataContract expected = new TestDataContract();
            TestDataContract actual;
            actual = target.ReadObject(stream);
            Assert.AreEqual<TestDataContract>(expected, actual);
        }

        [DataContract(
            Name = "TestDataContractAlias",
            Namespace = "http://schemas.datacontract.org/2004/07/System.ServiceModel")]
        class TestDataContract : IEquatable<TestDataContract>
        {
            public const string SER_DATA = @"<TestDataContractAlias xmlns=""http://schemas.datacontract.org/2004/07/System.ServiceModel"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance""><PrivateField>Private Field Value</PrivateField><PublicField>Public Field Value</PublicField></TestDataContractAlias>";

            [DataMember]
            public string PublicField;
            [DataMember]
            public string PrivateField;
            public string NotSerialized;

            public TestDataContract()
            {
                PublicField = "Public Field Value";
                PrivateField = "Private Field Value";
                NotSerialized = "Not Serialized";
            }

            #region IEquatable<TestDataContract> Members

            public bool Equals(TestDataContract other)
            {
                if (
                    other.PublicField == PublicField &&
                    other.PrivateField == PrivateField
                    )
                {
                    return true;
                }
                return false;
            }
            public override bool Equals(Object obj)
            {
                if (obj == null) return base.Equals(obj);
                if (!(obj is TestDataContract))
                    throw new InvalidCastException("Not a TestDataContract object.");
                else
                    return Equals(obj as TestDataContract);
            }
            public override int GetHashCode()
            {
                return this.PublicField.GetHashCode() ^ this.PrivateField.GetHashCode();
            }

            #endregion
        }


        /// <summary>
        /// Basic object to serialize and deserialize
        /// </summary>
        [DataContract]
        public class DataObject
        {
            [DataMember]
            public int Value { get; set; }
        }

        [TestMethod]
        public void SerializeAndDeserialize()
        {
            DataObject o = new DataObject();
            o.Value = 432;
            string xml = DataContractSerializer<DataObject>.Serialize(o);
            Trace.WriteLine(XDocument.Parse(xml).ToString());
            o = DataContractSerializer<DataObject>.Deserialize(xml);
            Assert.AreEqual(432, o.Value);
        }
    }


}
