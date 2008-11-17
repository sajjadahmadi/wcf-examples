using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Collections.ObjectModel;
using System.ServiceModel.Channels;

namespace System.ServiceModel.Errors
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ErrorHandlerBehaviorAttribute : Attribute, IErrorHandler, IServiceBehavior
    {
        #region IServiceBehavior Members
        protected Type ServiceType { get; set; }

        void IServiceBehavior.AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters) { }
        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription description, ServiceHostBase host)
        {
            ServiceType = description.ServiceType;
            foreach (ChannelDispatcher dispatcher in host.ChannelDispatchers)
            { dispatcher.ErrorHandlers.Add(this); }
        }
        void IServiceBehavior.Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase) { }
        #endregion

        #region IErrorHandler Members

        bool IErrorHandler.HandleError(Exception error)
        {
            // TODO: Handle error here!
            return false;
        }

        void IErrorHandler.ProvideFault(Exception error, System.ServiceModel.Channels.MessageVersion version, ref System.ServiceModel.Channels.Message fault)
        {
            ErrorHandlerHelper.PromoteException(ServiceType, error, version, ref fault);
        }

        #endregion
    }
}
