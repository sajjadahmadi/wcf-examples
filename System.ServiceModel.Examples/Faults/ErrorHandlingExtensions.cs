using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Description;
using System.ServiceModel;
using System.Collections.ObjectModel;

class BasicErrorHandlerBehaviorAttribute : Attribute, IErrorHandler, IServiceBehavior
{
    internal bool provideFaultCalled;

    #region IErrorHandler Members

    public bool HandleError(Exception error)
    { return false; }

    public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
    {
        provideFaultCalled = true;
    }

    #endregion

    #region IServiceBehavior Members

    public void AddBindingParameters(ServiceDescription description, ServiceHostBase host, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
    { }
    public void ApplyDispatchBehavior(ServiceDescription description, ServiceHostBase host)
    {
        foreach (ChannelDispatcher dispatcher in host.ChannelDispatchers)
        { dispatcher.ErrorHandlers.Add(this); }
    }
    public void Validate(ServiceDescription description, ServiceHostBase host)
    { }

    #endregion
}