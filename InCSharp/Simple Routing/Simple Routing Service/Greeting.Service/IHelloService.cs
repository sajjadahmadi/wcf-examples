using System.ServiceModel;

namespace Greeting.Service
{
    [ServiceContract]
    public interface IHelloService
    {
        [OperationContract(Action = "Hello")]
        string SayHello();
    }
}