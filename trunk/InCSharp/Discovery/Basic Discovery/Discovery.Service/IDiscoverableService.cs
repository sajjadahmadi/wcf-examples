using System.ServiceModel;

namespace Discovery.Service
{
    [ServiceContract]
    public interface IDiscoverableService
    {
        [OperationContract(IsOneWay = true)]
        void ServiceOperation();
    }
}