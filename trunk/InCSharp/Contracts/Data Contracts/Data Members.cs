using System.Runtime.Serialization;

namespace WcfExamples
{
    [DataContract(Namespace =
        "http://schemas.consumersenergy.com/2008/12/wcfexamples/")]
    public class MemberInfo
    {
        [DataMember(
            Name = "MemberAlias",
            EmitDefaultValue = false,
            IsRequired = true,
            Order = 0)]
        public string MemberString;
    }
}
