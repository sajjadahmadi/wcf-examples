using System.ServiceModel;

namespace Greeting.Service
{
    [ServiceContract]
    public interface IGoodbyeService
    {
        [OperationContract(Action = "Goodbye")]
        string SayGoodbye();
    }
}