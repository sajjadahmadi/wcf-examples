using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.ServiceModel.Extensions;

namespace System.ServiceModel
{
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
