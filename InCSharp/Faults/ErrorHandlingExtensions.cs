using System.Collections.ObjectModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Errors
{
    public interface IErrorHandlerBehavior : IErrorHandler, IServiceBehavior
    {
        Type ServiceType { get; }
    }

    public class ErrorHandlerBehavior : IErrorHandlerBehavior
    {
        IErrorHandler handler;

        public ErrorHandlerBehavior(IErrorHandler handler)
        { this.handler = handler; }

        #region IErrorHandler Members
        bool IErrorHandler.HandleError(Exception error)
        { return handler.HandleError(error); }
        void IErrorHandler.ProvideFault(Exception error, MessageVersion version, ref Message fault)
        { handler.ProvideFault(error, version, ref fault); }
        #endregion

        #region IServiceBehavior Members
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
        #endregion

        #region IErrorHandlerBehavior Members

        Type serviceType;
        Type IErrorHandlerBehavior.ServiceType
        {
            get { return serviceType; }
        }

        #endregion
    }

    // Attribute Wrapper 
    public class ErrorHandlerBehaviorAttribute : Attribute, IErrorHandlerBehavior
    {
        IErrorHandlerBehavior behavior;

        public ErrorHandlerBehaviorAttribute(IErrorHandlerBehavior behavior)
        { this.behavior = behavior; }

        #region IErrorHandler Members

        public bool HandleError(Exception error)
        { return behavior.HandleError(error); }

        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        { behavior.ProvideFault(error, version, ref fault); }

        #endregion

        #region IServiceBehavior Members

        public void AddBindingParameters(ServiceDescription description, ServiceHostBase host, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        { behavior.AddBindingParameters(description, host, endpoints, bindingParameters); }
        public void ApplyDispatchBehavior(ServiceDescription description, ServiceHostBase host)
        { behavior.ApplyDispatchBehavior(description, host); }
        public void Validate(ServiceDescription description, ServiceHostBase host)
        { behavior.Validate(description, host); }

        #endregion

        #region IErrorHandlerBehavior Members

        public Type ServiceType
        {
            get { return behavior.ServiceType; }
        }

        #endregion
    }
}