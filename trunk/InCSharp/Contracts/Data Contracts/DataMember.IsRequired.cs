using System;
using System.Runtime.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WcfExamples
{
    [TestClass]
    public class IsRequired
    {
        [TestMethod]
        [ExpectedException(typeof (SerializationException))]
        public void MissingMemberThatIsRequired()
        {
            const string missingMember =
                "<DataMemberExample.DataMemberIsRequired xmlns=\"http://schemas.datacontract.org/2004/07/WcfExamples\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" />";
            var obj = DataContractSerializer<DataMemberIsRequired>.Deserialize(missingMember);
        }

        #region Nested type: DataMemberIsRequired

        [DataContract]
        public class DataMemberIsRequired
        {
            [DataMember(IsRequired = true)] public string MemberString;
        }

        #endregion
    }
}