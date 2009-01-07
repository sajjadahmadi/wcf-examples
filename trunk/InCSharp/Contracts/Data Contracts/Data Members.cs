using System.Runtime.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Xml.Linq;
using System;

namespace WcfExamples
{
    [TestClass]
    public class DataMemberExample
    {
        [DataContract]
        public class DataMemberIsRequired
        {
            [DataMember(IsRequired = true)]
            public string MemberString;
        }

        [DataContract]
        public class DataMemberEmitDefaultValueFalse
        {
            [DataMember(EmitDefaultValue = false)]
            public string MemberString;
        }


        [TestMethod]
        public void EmitDefaultValueFalse() {
            var data = new DataMemberEmitDefaultValueFalse();
            var s = DataContractSerializer<DataMemberEmitDefaultValueFalse>.Serialize(data);
            var doc = XDocument.Parse(s);
            string expected = "<DataMemberExample.DataMemberEmitDefaultValueFalse xmlns=\"http://schemas.datacontract.org/2004/07/WcfExamples\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" />";
            Assert.AreEqual(expected, doc.ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(SerializationException))]
        public void MissingMemberThatIsRequired() {
            var missingMember = "<DataMemberExample.DataMemberIsRequired xmlns=\"http://schemas.datacontract.org/2004/07/WcfExamples\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" />";
            var obj = DataContractSerializer<DataMemberIsRequired>.Deserialize(missingMember);
        }
    }
}
