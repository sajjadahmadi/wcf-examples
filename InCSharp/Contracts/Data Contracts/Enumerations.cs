using System.Runtime.Serialization;

namespace WcfExamples
{
    [DataContract(Namespace =
        "http://schemas.consumersenergy.com/2008/12/wcfexamples/")]
    public enum MyEnumeration : int
    {
        [EnumMember]
        One = 1,

        [EnumMember]
        Two = 2,

        [EnumMember]
        Three = 3
    }
}
