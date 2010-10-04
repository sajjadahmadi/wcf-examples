using System.ServiceModel;

namespace Greeting.Client
{
    [ServiceContract]
    internal interface IGreetingService
    {
        [OperationContract(Action = "Hello")]
        string SayHello();

        [OperationContract(Action = "Goodbye")]
        string SayGoodbye();        
    }
}