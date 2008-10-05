using System.Runtime.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;

namespace System.ServiceModel
{
    /// <summary>
    ///This is a test class for DataContractSerializerTest and is intended
    ///to contain all DataContractSerializerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DataContractSerializerTest
    {
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

        [TestMethod()]
        public void WriteObjectTest()
        {
            DataContractSerializer<TestDataContract> serializer = new DataContractSerializer<TestDataContract>();
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
    }


}
