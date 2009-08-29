using System.Runtime.Serialization;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WcfExamples
{
    [TestClass]
    public class EmitDefaultValue
    {
        [TestMethod]
        public void EmitDefaultValueFalse()
        {
            var data = new DataMemberEmitDefaultValueFalse();
            var serData = DataContractSerializer<DataMemberEmitDefaultValueFalse>.Serialize(data);
            const string exp = "<EmitDefaultValue.DataMemberEmitDefaultValueFalse xmlns=\"http://schemas.datacontract.org/2004/07/WcfExamples\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" />";
            Assert.AreEqual(exp, serData);
        }

        [TestMethod]
        public void EmitDefaultValueTrue()
        {
            var data = new DataMemberEmitDefaultValueTrue();
            var serData = DataContractSerializer<DataMemberEmitDefaultValueTrue>.Serialize(data);
            const string exp = "<EmitDefaultValue.DataMemberEmitDefaultValueTrue xmlns=\"http://schemas.datacontract.org/2004/07/WcfExamples\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\"><MemberString i:nil=\"true\"/></EmitDefaultValue.DataMemberEmitDefaultValueTrue>";
            Assert.AreEqual(exp, serData);
        }

        #region Nested type: DataMemberEmitDefaultValueFalse

        [DataContract]
        public class DataMemberEmitDefaultValueFalse
        {
            [DataMember(EmitDefaultValue = false)]
            public string MemberString;
        }

        [DataContract]
        public class DataMemberEmitDefaultValueTrue
        {
            [DataMember(EmitDefaultValue = true)]
            public string MemberString;
        }
        #endregion
    }
}