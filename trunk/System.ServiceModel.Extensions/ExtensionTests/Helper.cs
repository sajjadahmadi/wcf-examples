using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace System.ServiceModel
{
    class Helper
    {
        public static string Serialize<T>(T dataContract)
        {
            DataContractSerializer<T> formatter = new DataContractSerializer<T>();
            using (Stream stream = new MemoryStream())
            {
                formatter.WriteObject(stream, dataContract);
                stream.Position = 0;
                StreamReader reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
        }
        public static T Deserialize<T>(string xmlData)
        {
            T obj;
            DataContractSerializer<T> formatter = new DataContractSerializer<T>();
            using (Stream stream = new MemoryStream(Text.Encoding.Default.GetBytes(xmlData)))
            {
                obj = formatter.ReadObject(stream);
            }
            return obj;
        }
    }

    [DataContract]
    public class DataObject
    {
        [DataMember]
        public int Value { get; set; }
    }

    [TestClass]
    public class HelperTests
    {
        [TestMethod]
        public void Test()
        {
            DataObject o = new DataObject();
            o.Value=432;
            string xml = Helper.Serialize<DataObject>(o);
            Trace.WriteLine(XDocument.Parse(xml).ToString());
            o = Helper.Deserialize<DataObject>(xml);
            Assert.AreEqual(432, o.Value);
        }
    }
}
