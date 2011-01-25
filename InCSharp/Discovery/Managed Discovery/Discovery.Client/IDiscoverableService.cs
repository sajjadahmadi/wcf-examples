using System.ServiceModel;

namespace Discovery.Client
{
    [ServiceContract]
    public interface IDiscoverableService
    {
        [OperationContract(IsOneWay = true)]
        void ServiceOperation();
    }
}