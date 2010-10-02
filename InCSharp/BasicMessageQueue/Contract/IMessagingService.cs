using System.ServiceModel;

namespace QueuedCalls.Contract
{
    [ServiceContract(Namespace = "http://www.mynamespace.com/")]
    public interface IMessagingService
    {
        [OperationContract(IsOneWay = true)]
        void SendMessage(string message);
    }
}