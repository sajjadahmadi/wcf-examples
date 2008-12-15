using System.Collections.ObjectModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Errors
{
    public class PromoteExceptionBehavior : IErrorHandlerBehavior
    {
        Type serviceType;
        Type IErrorHandlerBehavior.ServiceType
        {
            get { return serviceType; }
        }

        bool IErrorHandler.HandleError(Exception error)
        {
            // TODO: Handle or log error here!
            return false;
        }

        void IErrorHandler.ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            ErrorHandlerHelper.PromoteException(serviceType, error, version, ref fault);
        }

        void IServiceBehavior.AddBindingParameters(ServiceDescription description, ServiceHostBase host, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        { }
        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription description, ServiceHostBase host)
        {
            serviceType = description.ServiceType;
            foreach (ChannelDispatcher dispatcher in host.ChannelDispatchers)
            { dispatcher.ErrorHandlers.Add(this); }
        }
        void IServiceBehavior.Validate(ServiceDescription description, ServiceHostBase host)
        { }
    }
}