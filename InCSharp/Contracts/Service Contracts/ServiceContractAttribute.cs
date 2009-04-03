using System.ServiceModel;

namespace WcfExamples
{
    /// <summary>
    /// Contract
    /// Use Name to remove the I, because the I is a .NET idiom
    /// Use Namespace to provide a unique URI.  Follow W3C convension
    /// and use year and month to differentiate versions of your service.
    /// </summary>
    [ServiceContract(Name = "MyContract",
        Namespace = "http://www.cmsenergy.com/2008/12/wcfexamples/")]
    public interface IMyContract
    {
        [OperationContract]
        int MyMethod();
    }
}
