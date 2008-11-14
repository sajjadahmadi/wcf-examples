using System.ServiceModel.Channels;

namespace System.ServiceModel.Examples
{
    [ServiceContract]
    interface IInstanceIdGetter
    {
        [OperationContract]
        Guid GetInstanceId();
    }

    class InstanceIdSetter
    {
        protected Guid instanceId = Guid.NewGuid();
    }

    class ServiceClient : ClientBase<IInstanceIdGetter>, IInstanceIdGetter
    {
        public ServiceClient(Binding binding, string remoteAddress)
            : base(binding, new EndpointAddress(remoteAddress)) { }
        public Guid GetInstanceId()
        { return Channel.GetInstanceId(); }
    }
}
